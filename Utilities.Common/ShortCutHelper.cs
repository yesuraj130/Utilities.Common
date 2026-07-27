using Microsoft.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Utilities.Common
{
    public static class ShortCutHelper
    {
        private const string WshGuid = "72C24DD5-D70A-438B-8A42-98424B88AFB8";
        private const string registryStartUpPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\StartupFolder";
        private static readonly string startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        public static void CreateInStartUpFolder(string ShortCutName, string targetPath, string description = null, string argument = null, string startIn = null)
        {
            var linkPath = startupFolderPath + @"\" + ShortCutName + ".lnk";

            AddShortCut(linkPath, targetPath, description, argument, startIn);
        }
        public static bool ExistsInStartUpFolder(string ShortCutName, string targetPath)
        {
            var linkPath = startupFolderPath + @"\" + ShortCutName + ".lnk";

            if (File.Exists(linkPath)) return Exists(linkPath, targetPath);
            else return false;
        }
        public static bool DeleteInStartUpFolder(string ShortCutName)
        {
            var linkPath = startupFolderPath + @"\" + ShortCutName + ".lnk";

            if (File.Exists(linkPath))
            {
                File.Delete(linkPath);
                return true;
            }

            return false;
        }

        public static bool IsEnabledInStartUpFolder(string ShortCutName)
        {
            using (var startUpKey = Registry.CurrentUser.OpenSubKey(registryStartUpPath, false))
            {
                if (startUpKey is not null)
                {
                    var appKeyValue = startUpKey.GetValue(ShortCutName + ".lnk");
                    return appKeyValue is byte[] appKeyValueByte && appKeyValueByte.Length == 12 && appKeyValueByte[0] == 0x02;
                }
            }

            return false;
        }
        public static void EnableInStartUpFolder(string ShortCutName)
        {
            using (var startUpKey = Registry.CurrentUser.OpenSubKey(registryStartUpPath, true))
            {
                if (startUpKey is not null)
                {
                    startUpKey.SetValue(ShortCutName + ".lnk", new byte[] { 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                }
            }

        }

        private static void AddShortCut(string fullPathToLink, string fullPathToTargetExe, string description = null, string argument = null, string startIn = null)
        {
            dynamic shell = Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid(WshGuid)));
            try
            {
                dynamic shortCut = shell.CreateShortcut(fullPathToLink);
                try
                {
                    if (File.Exists(fullPathToLink))
                    {
                        File.Delete(fullPathToLink);
                    }

                    //val2.IconLocation = fullPathToTargetExe;
                    shortCut.TargetPath = fullPathToTargetExe;
                    if (argument != null)
                    {
                        shortCut.Arguments = argument;
                    }

                    if (description != null)
                    {
                        shortCut.Description = description;
                    }

                    if (startIn != null)
                    {
                        shortCut.WorkingDirectory = startIn;
                    }
                    shortCut.Save();
                }
                finally
                {
                    Marshal.FinalReleaseComObject(shortCut);
                }
            }
            finally
            {
                Marshal.FinalReleaseComObject(shell);
            }
        }
        private static bool Exists(string fullPathToLink, string fullPathToTargetExe)
        {
            dynamic val = Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid(WshGuid)));
            try
            {
                dynamic val2 = val.CreateShortcut(fullPathToLink);
                try
                {
                    return val2.TargetPath == fullPathToTargetExe;
                }
                finally
                {
                    Marshal.FinalReleaseComObject(val2);
                }
            }
            finally
            {
                Marshal.FinalReleaseComObject(val);
            }
        }
        public static string GetTargetPath(string fullPathToLink)
        {
            dynamic val = Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid(WshGuid)));
            try
            {
                dynamic val2 = val.CreateShortcut(fullPathToLink);
                try
                {
                    return val2.TargetPath;
                }
                finally
                {
                    Marshal.FinalReleaseComObject(val2);
                }
            }
            finally
            {
                Marshal.FinalReleaseComObject(val);
            }
        }
    }

}
