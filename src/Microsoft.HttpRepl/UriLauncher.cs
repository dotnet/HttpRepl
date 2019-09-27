// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Resources;

namespace Microsoft.HttpRepl
{
    internal class UriLauncher : IUriLauncher
    {
        public Task LaunchUriAsync(Uri uri)
        {
            string agent;
            string agentParam;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                agent = "cmd";
                agentParam = $"/c start {uri.AbsoluteUri}";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                agent = "open";
                agentParam = uri.AbsoluteUri;
            }
            else // Linux
            {
                agent = "xdg-open";
                agentParam = uri.AbsoluteUri;
            }

            Process process = Process.Start(new ProcessStartInfo(agent, agentParam) { CreateNoWindow = true });

            if (process != null)
            {
                return Task.CompletedTask;
            }
            else
            {
                string uriLaunchErrorMessage = string.Format(Strings.UICommand_UnableToLaunchUriError, uri);

                return Task.FromException(new InvalidOperationException(uriLaunchErrorMessage));
            }
        }
    }
}
