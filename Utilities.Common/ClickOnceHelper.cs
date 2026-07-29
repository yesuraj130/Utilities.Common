using System;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Utilities.Common.Extensions;

namespace Utilities.Common
{
    /// <summary>
    /// Helper utilities around ClickOnce deployed applications.
    /// Improvements: safer disposal, async update checks, robust Process.Start handling and watcher safety.
    /// </summary>
    public class ClickOnceHelper : IDisposable
    {
        //private readonly string publisher;
        private readonly string appName;
        private readonly string networkPath;
        private readonly string programsPath;

        // watcher may be accessed from different threads; keep reference volatile to reduce races.
        private volatile FileSystemWatcherExt watcher;
        private bool disposed;

        public ClickOnceHelper(string publisher, string appName, string networkPath)
        {
            //this.publisher = publisher;
            this.appName = appName ?? throw new ArgumentNullException(nameof(appName));
            this.networkPath = networkPath;
            this.programsPath = GetProgramsPath(publisher, appName);
        }

        /// <summary>
        /// Result enum for click-once update attempts.
        /// </summary>
        public enum UpdateResult
        {
            Updated,
            UpToDate,
            NotClickOnce,
            Error
        }

        /// <summary>
        /// Async non-blocking version of the update check/installation.
        /// Returns a result enum and a human-friendly status message.
        /// </summary>
        public static async Task<(UpdateResult Result, string Status)> UpdateClickOnceAsync()
        {
            if (!ApplicationDeployment.IsNetworkDeployed)
            {
                return (UpdateResult.NotClickOnce, "This is not a ClickOnce application.");
            }

            var deployment = ApplicationDeployment.CurrentDeployment;

            try
            {
                // CheckForUpdate can perform network I/O. Run on background thread so callers (UI) won't block.
                bool updateAvailable = await Task.Run(() => deployment.CheckForUpdate()).ConfigureAwait(false);

                if (!updateAvailable)
                {
                    return (UpdateResult.UpToDate, "The application is up to date.");
                }

                var updateSuccess = await Task.Run(() => deployment.Update()).ConfigureAwait(false);
                if (updateSuccess)
                {
                    return (UpdateResult.Updated, "The application has been upgraded.");
                }
                else
                {
                    return (UpdateResult.Error, "Failed to update the application.");
                }
            }
            catch (DeploymentDownloadException dde)
            {
                return (UpdateResult.Error, "The application unable check for the existence of a new version at this time. \n\nPlease check your network connection, or try again later. \n\nError: " + dde.Message);
            }
            catch (InvalidDeploymentException ide)
            {
                return (UpdateResult.Error, "The application cannot check for an update. \n\nThe ClickOnce deployment is corrupt. Please re-install the application and try again. \n\nError: " + ide.Message);
            }
            catch (InvalidOperationException ioe)
            {
                return (UpdateResult.Error, "This application cannot check for an update. \n\nThis most often happens if the application is already in the process of updating. \n\nError: " + ioe.Message);
            }
            catch (TrustNotGrantedException tnge)
            {
                return (UpdateResult.Error, "The application cannot be updated. The system did not grant the application the appropriate level of trust. Please contact your system administrator or help desk for assistance. \n\nError: " + tnge.Message);
            }
            catch (Exception ex)
            {
                return (UpdateResult.Error, "An unexpected error occurred while checking for updates. \n\nError: " + ex.Message);
            }
        }

        /// <summary>
        /// Synchronous compatibility wrapper for existing callers.
        /// Note: This wrapper runs the async implementation on a background thread. Prefer UpdateClickOnceAsync.
        /// </summary>
        public static bool? UpdateClickOnce(out string status)
        {
            var task = Task.Run(UpdateClickOnceAsync);
            try
            {
                var (result, message) = task.GetAwaiter().GetResult();
                status = message;
                return result switch
                {
                    UpdateResult.Updated => true,
                    UpdateResult.UpToDate => false,
                    UpdateResult.NotClickOnce => null,
                    UpdateResult.Error => null,
                    _ => null
                };
            }
            catch (Exception ex)
            {
                status = "Failed to check or apply update: " + ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Attempts to start the ClickOnce deployment's UpdateLocation.
        /// Handles both file:// and network/http URIs robustly.
        /// </summary>
        public static bool StartClickOnceFromDeployment()
        {
            if (!ApplicationDeployment.IsNetworkDeployed) return false;

            try
            {
                var deployment = ApplicationDeployment.CurrentDeployment;
                var uri = deployment.UpdateLocation;
                if (uri == null) return false;

                // If the update location is a file URI (file://), use LocalPath and verify file exists.
                if (uri.IsFile)
                {
                    var path = uri.LocalPath;
                    if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    }

                    return false;
                }

                // For non-file URIs (http/https), try opening the URI (this will open the default browser).
                try
                {
                    Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool StartClickOnceFromPrograms()
        {
            try
            {
                if (!string.IsNullOrEmpty(programsPath) && File.Exists(programsPath))
                {
                    Process.Start(new ProcessStartInfo(programsPath) { UseShellExecute = true });
                    return true;
                }
            }
            catch
            {
                // swallow or optionally log
            }

            return false;
        }

        public bool StartClickOnceApplicationFromNetworkPath()
        {
            try
            {
                if (!string.IsNullOrEmpty(networkPath) && File.Exists(networkPath))
                {
                    Process.Start(new ProcessStartInfo(networkPath) { UseShellExecute = true });
                    return true;
                }
            }
            catch
            {
                // swallow or optionally log
            }

            return false;
        }

        /// <summary>
        /// Starts a file watcher that monitors the network path directory for changes and triggers update checks.
        /// </summary>
        public async void StartUpdateChecker()
        {
            // If an immediate update succeeds, raise the event and do not start watcher.
            var (result, status) = await UpdateClickOnceAsync().ConfigureAwait(false);
            if (result == UpdateResult.Updated)
            {
                ApplicationUpdated?.Invoke(this, EventArgs.Empty);
                return;
            }

            // If watcher already active, don't create another.
            if (watcher is not null) return;

            if (string.IsNullOrEmpty(networkPath)) return;

            try
            {
                // Only create watcher if the directory exists (avoid exceptions)
                var dir = Path.GetDirectoryName(networkPath);
                if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return;

                watcher = new FileSystemWatcherExt
                {
                    Path = dir,
                    IncludeSubdirectories = false,
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Security | NotifyFilters.Attributes | NotifyFilters.FileName,
                    // Watch for application files; keep as-is or adjust if needed
                    Filter = "*.application",
                    TimeOut = TimeSpan.FromMinutes(1),
                };
                watcher.Changed += FileWatcherChangeDetected;
                watcher.EnableRaisingEvents = true;
            }
            catch
            {
                // swallow or optionally log
            }
        }

        private async void FileWatcherChangeDetected(object sender, FileSystemEventArgs e)
        {
            // Use a local copy to reduce race conditions if watcher is disposed concurrently.
            var localWatcher = watcher;
            if (localWatcher == null) return;

            try
            {
                // Disable events on the watcher we captured.
                try { localWatcher.EnableRaisingEvents = false; } catch { /* ignore */ }

                // Delay a bit to allow write operations to complete on the deploy server.
                await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);

                var (result, status) = await UpdateClickOnceAsync().ConfigureAwait(false);
                if (result == UpdateResult.Updated)
                {
                    try { localWatcher.Dispose(); } catch { /* ignore */ }

                    // Only clear the field if it's the same watcher instance we disposed.
                    if (ReferenceEquals(localWatcher, watcher))
                        watcher = null;

                    ApplicationUpdated?.Invoke(this, EventArgs.Empty);
                    return;
                }

                // Re-enable only the watcher instance we disabled.
                if (ReferenceEquals(localWatcher, watcher))
                {
                    try { localWatcher.EnableRaisingEvents = true; } catch { /* ignore */ }
                }
            }
            catch
            {
                // Ensure we don't let exceptions escape an async void event handler (which would crash the process).
                try
                {
                    if (ReferenceEquals(localWatcher, watcher)) localWatcher.EnableRaisingEvents = true;
                }
                catch { }
            }
        }

        public event EventHandler ApplicationUpdated;

        /// <summary>
        /// Adds an autostart shortcut in Startup if not present or if disabled. Returns:
        /// - null: cannot create because shortcut target doesn't exist
        /// - true: autostart was created or enabled
        /// - false: autostart already exists and is enabled
        /// </summary>
        public bool? AddAutoStart(string autoStartArgument = null)
        {
            if (!IsAutoStartTargetExists()) return null;

            try
            {
                if (!IsAutoStartExists())
                {
                    ShortCutHelper.CreateInStartUpFolder(appName, programsPath, null, autoStartArgument);
                    return true;
                }

                if (!IsAutoStartEnabled())
                {
                    AutoStartEnable();
                    return true;
                }
            }
            catch
            {
                // If ShortCutHelper throws, treat as failure (null)
                return null;
            }

            return false;
        }

        public bool IsAutoStartExists() => ShortCutHelper.ExistsInStartUpFolder(appName, programsPath);
        public bool IsAutoStartTargetExists() => !string.IsNullOrEmpty(programsPath) && File.Exists(programsPath);
        public bool IsAutoStartEnabled() => IsAutoStartExists() && ShortCutHelper.IsEnabledInStartUpFolder(appName);
        private void AutoStartEnable() => ShortCutHelper.EnableInStartUpFolder(appName);

        public bool RemoveAutoStart()
        {
            try
            {
                return ShortCutHelper.DeleteInStartUpFolder(appName);
            }
            catch
            {
                return false;
            }
        }

        private static string GetProgramsPath(string publisher, string appName)
        {
            if (string.IsNullOrWhiteSpace(appName))
                throw new ArgumentException("appName must be provided", nameof(appName));

            var programsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            var startAppPath = Path.Combine(programsFolder, publisher ?? string.Empty, $"{appName}.appref-ms");

            try
            {
                if (File.Exists(startAppPath)) return startAppPath;

                var dir = Path.GetDirectoryName(startAppPath);
                if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return startAppPath;

                var files = Directory.GetFiles(dir, $"{appName}*.appref-ms");
                if (files.Length > 0) return files[0];

                return startAppPath;
            }
            catch
            {
                // If anything goes wrong, return the constructed candidate path to allow caller checks.
                return startAppPath;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                try
                {
                    watcher?.Dispose();
                }
                catch
                {
                    // swallow or log
                }
                watcher = null;
            }

            disposed = true;
        }

        ~ClickOnceHelper()
        {
            Dispose(false);
        }
    }
}
