using System;
using System.Deployment.Application;
using System.IO;
using System.Threading.Tasks;
using Utilities.Common.Extensions;

namespace Utilities.Common
{
    public class ClickOnceHelper : IDisposable
    {
        //private readonly string publisher;
        private readonly string appName;
        private readonly string networkPath;
        private readonly string programsPath;
        private FileSystemWatcherExt watcher;



        public ClickOnceHelper(string publisher, string appName, string networkPath)
        {
            //this.publisher = publisher;
            this.appName = appName;
            this.networkPath = networkPath;
            this.programsPath = GetProgramsPath(publisher, appName);

            static string GetProgramsPath(string publisher, string appName)
            {
                var startAppPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), publisher, $"{appName}.appref-ms");
                if (File.Exists(startAppPath)) return startAppPath;

                foreach (var item in Directory.GetFiles(Path.GetDirectoryName(startAppPath), $"{appName}*.appref-ms"))
                {
                    return item;
                }

                return startAppPath;
            }
        }

        public static bool? UpdateClickOnce(out string status)
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                var deployment = ApplicationDeployment.CurrentDeployment;

                bool updateAvailable;
                try
                {
                    updateAvailable = deployment.CheckForUpdate();
                }
                catch (DeploymentDownloadException dde)
                {
                    status = "The application unable check for the existence of a new version at this time. \n\nPlease check your network connection, or try again later. \n\nError: " + dde;
                    return null;
                }
                catch (InvalidDeploymentException ide)
                {
                    status = "The application cannot check for an update. \n\nThe ClickOnce deployment is corrupt. Please re-install the application and try again. \n\nError: " + ide.Message;
                    return null;
                }
                catch (InvalidOperationException ioe)
                {
                    status = "This application cannot check for an update. \n\nThis most often happens if the application is already in the process of updating. \n\nError: " + ioe.Message;
                    return null;
                }

                if (updateAvailable)
                {
                    try
                    {
                        var updateSuccess = deployment.Update();
                        if (updateSuccess)
                        {
                            status = "The application has been upgraded.";
                            return true;
                        }
                        else
                        {
                            status = "Failed to update the application";
                            return null;
                        }

                    }
                    catch (DeploymentDownloadException dde)
                    {
                        status = "Cannot install the latest version of the application. Either the deployment server is unavailable, or your network connection is down. \n\nPlease check your network connection, or try again later. Error: " + dde.Message;
                        return null;
                    }
                    catch (TrustNotGrantedException tnge)
                    {
                        status = "The application cannot be updated. The system did not grant the application the appropriate level of trust. Please contact your system administrator or help desk for further troubleshooting. Error: " + tnge.Message;
                        return null;
                    }
                }
                else
                {
                    status = "The application is up to date.";
                    return false;
                }
            }
            else
            {
                status = "This is not a ClickOnce application.";
                return null;
            }
        }
        public static bool StartClickOnceFromDeployment()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                var deployment = ApplicationDeployment.CurrentDeployment;
                if (File.Exists(deployment.UpdateLocation.AbsolutePath))
                {
                    System.Diagnostics.Process.Start(deployment.UpdateLocation.AbsolutePath);
                    return true;
                }
            }

            return false;
        }

        public bool StartClickOnceFromPrograms()
        {
            if (File.Exists(programsPath))
            {
                System.Diagnostics.Process.Start(programsPath);
                return true;
            }

            return false;
        }
        public bool StartClickOnceApplicationFromNetworkPath()
        {
            if (File.Exists(networkPath))
            {
                System.Diagnostics.Process.Start(networkPath);
                return true;
            }

            return false;
        }

        public void StartUpdateChecker()
        {
            if (UpdateClickOnce(out var status) is true)
            {
                ApplicationUpdated?.Invoke(this, EventArgs.Empty);
                return;
            }
            if (watcher is not null) return;

            if (File.Exists(networkPath))
            {
                //watcher?.Dispose();
                watcher = new FileSystemWatcherExt
                {
                    Path = Path.GetDirectoryName(networkPath),
                    IncludeSubdirectories = false,
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Security | NotifyFilters.Attributes | NotifyFilters.FileName,
                    Filter = "*.application",
                    TimeOut = TimeSpan.FromMinutes(1),
                };
                watcher.Changed += FileWatcherChangeDetected;

                watcher.EnableRaisingEvents = true;
            }
        }
        private async void FileWatcherChangeDetected(object sender, FileSystemEventArgs e)
        {
            watcher.EnableRaisingEvents = false;

            await Task.Delay(1000 * 10);

            if (UpdateClickOnce(out var status) is true)
            {
                watcher.Dispose();
                watcher = null;
                ApplicationUpdated?.Invoke(this, EventArgs.Empty);
                return;
            }
            watcher.EnableRaisingEvents = true;
        }

        public event EventHandler ApplicationUpdated;


        public bool? AddAutoStart(string autoStartArgument = null)
        {
            if (!IsAutoStartTargetExists()) return null;

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

            return false;
        }
        public bool IsAutoStartExists() => ShortCutHelper.ExistsInStartUpFolder(appName, programsPath);
        public bool IsAutoStartTargetExists() => File.Exists(programsPath);
        public bool IsAutoStartEnabled() => IsAutoStartExists() && ShortCutHelper.IsEnabledInStartUpFolder(appName);
        private void AutoStartEnable() => ShortCutHelper.EnableInStartUpFolder(appName);

        public bool RemoveAutoStart()
        {
            return ShortCutHelper.DeleteInStartUpFolder(appName);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool isDisposed) => watcher.Dispose();
    }

}
