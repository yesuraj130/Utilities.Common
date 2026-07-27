using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utilities.Common.Extensions
{
    public static class FileExt
    {
        public static void BackgroundCopy(string Source, string Destination)
        {
            _ = Task.Run(() =>
            {
                Thread.CurrentThread.IsBackground = false;
                try { Copy(Source, Destination); } catch { }
            });
        }

        public static bool IsExistsCopy(string Source, string Destination)
        {
            if (File.Exists(Source))
            {
                Copy(Source, Destination);
                return true;
            }
            return false;
        }

        public static bool IsLatestCopy(string Source, string Destination)
        {
            if (File.Exists(Destination))
            {
                if (File.Exists(Source) && File.GetLastWriteTime(Source) != File.GetLastWriteTime(Destination))
                {
                    //File.Copy(Source, Destination,true);
                    CheckReadOnly(Destination);
                    Copy(Source, Destination);
                    return true;
                }

            }
            else
            {
                if (File.Exists(Source))
                {
                    Copy(Source, Destination);
                    return true;
                }
            }
            return false;
        }
        public static bool IsLatestCopyWithoutLock(string Source, string Destination)
        {
            if (File.Exists(Destination))
            {
                if (File.Exists(Source) && File.GetLastWriteTime(Source) != File.GetLastWriteTime(Destination))
                {
                    CheckReadOnly(Destination);
                    CopyWithoutLock(Source, Destination);
                    return true;
                }

            }
            else
            {
                if (File.Exists(Source))
                {
                    Copy(Source, Destination);
                    return true;
                }
            }
            return false;
        }
        public static bool CheckIsLatest(string Source, string Destination)
        {
            if (File.Exists(Destination))
            {
                if (File.Exists(Source) && File.GetLastWriteTime(Source) != File.GetLastWriteTime(Destination))
                {
                    return true;
                }

            }
            else
            {
                if (File.Exists(Source))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsLatestCopyWithoutCheck(string Source, string Destination)
        {
            if (File.Exists(Destination))
            {
                if (File.GetLastWriteTime(Source) != File.GetLastWriteTime(Destination))
                {
                    CheckReadOnly(Destination);
                    File.Copy(Source, Destination, true);
                    return true;
                }
            }
            else
            {
                File.Copy(Source, Destination, true);
                return true;
            }
            return false;
        }

        /// <summary> Create directory before copying </summary>
        public static void Copy(string Source, string Destination)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Destination));
            //CopyWithoutLock(Source, Destination);
            CheckReadOnly(Destination);
            File.Copy(Source, Destination, true);
        }

        public static void Copy(string Source, string Destination, int timeout)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Destination));
            try
            {
                //CopyWithoutLock(Source, Destination);
                CheckReadOnly(Destination);
                File.Copy(Source, Destination, true);
            }
            catch (IOException)
            {
                Thread.Sleep(timeout);
                //CopyWithoutLock(Source, Destination);
                CheckReadOnly(Destination);
                File.Copy(Source, Destination, true);
            }
        }

        private static void CheckReadOnly(string Destination)
        {
            try
            {
                var attrs = File.GetAttributes(Destination);
                if (attrs.HasFlag(FileAttributes.ReadOnly)) File.SetAttributes(Destination, attrs & ~FileAttributes.ReadOnly);
            }
            catch { }
        }

        /// <summary> Copy file without locking source. Does not matches LastWriteTime </summary>
        public static void CopyWithoutLock(string Source, string Destination)
        {

            Directory.CreateDirectory(Path.GetDirectoryName(Destination));
            using (FileStream fileStream = new FileStream(Source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (FileStream destStream = new FileStream(Destination, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                {
                    fileStream.CopyTo(destStream);
                }
            }


        }

        public static string CreateBackupName(string OriginalName)
        {
            return Path.GetFileNameWithoutExtension(OriginalName) + " - " + DateTime.Now.ToString("dd MMM yyyy HH.mm.ss") + " - " + Environment.UserName + Path.GetExtension(OriginalName);
        }

        public static bool IsPathEquals(string path1, string path2)
        {
            return Path.GetFullPath(path1).TrimEnd(Path.DirectorySeparatorChar).Equals(Path.GetFullPath(path2).TrimEnd(Path.DirectorySeparatorChar), StringComparison.InvariantCultureIgnoreCase);
        }

        public static void SetReadOnly(string filePath, bool newValue)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            if (fileInfo.IsReadOnly != newValue)
            {
                fileInfo.IsReadOnly = newValue;
            }
        }

        //[DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        //private extern static bool PathFileExists(StringBuilder path);

        //public static bool Exists(string path)
        //{
        //    // A StringBuilder is required for interops calls that use strings
        //    var builder = new StringBuilder(path);
        //    return PathFileExists(builder);
        //}
    }
}
