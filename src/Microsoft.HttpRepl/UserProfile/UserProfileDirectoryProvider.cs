// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
