using System;
using System.Runtime.InteropServices;

namespace Microsoft.HttpRepl.UserProfile
{
    public class UserProfileDirectoryProvider : IUserProfileDirectoryProvider
    {
        public string GetUserProfileDirectory()
        {
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            string profileDir = Environment.GetEnvironmentVariable(isWindows
                ? "USERPROFILE"
                : "HOME");

            return profileDir;
        }
    }
}
