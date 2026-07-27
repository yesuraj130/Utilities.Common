using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Utilities.Common.Extensions
{

    public class FileSystemWatcherExt : FileSystemWatcher
    {

        public FileSystemWatcherExt() : base() => EnableAutoReconnect();
        public FileSystemWatcherExt(string path) : base(path) => EnableAutoReconnect();
        public FileSystemWatcherExt(string path, string filter) : base(path, filter) => EnableAutoReconnect();
        public TimeSpan TimeOut { get; set; } = new TimeSpan(0, 1, 0);

        private void EnableAutoReconnect() => this.Error += FileSystemWatcherExt_Error;
        private void FileSystemWatcherExt_Error(object sender, ErrorEventArgs e) => Task.Run(() => TryReconnect(TimeOut, this.Path, this));
        private async static void TryReconnect(TimeSpan TimeOut, string Path, FileSystemWatcherExt Watcher)
        {
            while (true)
            {
                await Task.Delay(TimeOut);
                if (Directory.Exists(Path))
                {
                    try
                    {
                        Watcher.EnableRaisingEvents = true;
                        if (Watcher.EnableRaisingEvents)
                        {
                            Watcher.ConnectionRestored?.Invoke(Watcher, EventArgs.Empty);
                            break;
                        }
                    }
                    catch { }
                }
            }
        }

        public event EventHandler ConnectionRestored;
    }
}
