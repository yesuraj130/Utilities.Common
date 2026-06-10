using System;
using System.Deployment.Application;
using System.IO;

namespace Utilities.Common.Extensions
{
    public static class ClickOnceExt
    {
        private const string NotInstalled = "Not-Installed";
        public static string ShortBuildVersion
        {
            get
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    var Version = InternalGetBuildVersion();
                    if (Version == null) return NotInstalled;

                    if (Version.Build == 0 && Version.Revision == 0) return Version.Major + "." + Version.Minor;
                    return Version.ToString();
                }
                else
                {
                    return NotInstalled;
                }

            }
        }


        public static string LongBuildVersion
        {
            get
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    var Version = InternalGetBuildVersion();
                    if (Version is null) return NotInstalled;
                    return Version.ToString();
                }
                else
                {
                    return NotInstalled;
                }
            }
        }
        public static Version BuildVersion
        {
            get
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    return InternalGetBuildVersion();
                }
                else
                {
                    return null;
                }
            }
        }
        private static Version InternalGetBuildVersion()
        {
            try
            {
                return ApplicationDeployment.CurrentDeployment.CurrentVersion;
            }
            catch (InvalidDeploymentException)
            {
                return null;
            }
        }
        public static bool IsInstalled
        {
            get
            {
                return ApplicationDeployment.IsNetworkDeployed;
            }

        }
        public static bool IsLatest
        {
            get
            {
                return !ApplicationDeployment.CurrentDeployment.CheckForUpdate();
            }
        }
    }

}
