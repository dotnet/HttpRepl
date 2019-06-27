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
            string agent = "cmd";
            string agentParam = $"/c start {uri.AbsoluteUri}";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                agent = "open";
                agentParam = uri.AbsoluteUri;
            }
            else if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
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

                return Task.FromException(new Exception(uriLaunchErrorMessage));
            }
        }
    }
}
