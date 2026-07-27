using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Utilities.Common.Extensions
{
    public static class ShortCut
    {
        private const string WshGuid = "72C24DD5-D70A-438B-8A42-98424B88AFB8";

        public static void AddShortCut(string fullPathToLink, string fullPathToTargetExe, string description = null, string argument = null, string startIn = null)
        {
            Type t = Type.GetTypeFromCLSID(new Guid(WshGuid));
            dynamic shell = Activator.CreateInstance(t);

            try
            {
                var link = shell.CreateShortcut(fullPathToLink);
                try
                {
                    if (File.Exists(fullPathToLink)) File.Delete(fullPathToLink);
                    link.IconLocation = fullPathToTargetExe;
                    link.TargetPath = fullPathToTargetExe;
                    if (argument != null) link.Arguments = argument;
                    if (description != null) link.Description = description;
                    link.WorkingDirectory = startIn ?? Path.GetDirectoryName(fullPathToTargetExe);
                    link.Save();
                }
                finally { Marshal.FinalReleaseComObject(link); }
            }
            finally { Marshal.FinalReleaseComObject(shell); }
        }

        public static bool Exists(string fullPathToLink, string fullPathToTargetExe)
        {

            Type t = Type.GetTypeFromCLSID(new Guid(WshGuid));
            dynamic shell = Activator.CreateInstance(t);

            try
            {
                var link = shell.CreateShortcut(fullPathToLink);
                try { return link.TargetPath == fullPathToTargetExe; }
                finally { Marshal.FinalReleaseComObject(link); }
            }
            finally { Marshal.FinalReleaseComObject(shell); }
        }
    }
}
