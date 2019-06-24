using System;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    internal class HttpStateHelpers
    {
        public static HttpState Create(string baseAddress, string path = null)
        {
            HttpState httpState = new HttpState();
            httpState.BaseAddress = new Uri(baseAddress);

            if (path != null)
            {
                string[] pathParts = path.Split('/');

                foreach (string pathPart in pathParts)
                {
                    httpState.PathSections.Push(pathPart);
                }
            }

            return httpState;
        }
    }
}
