// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.HttpRepl.Suggestions
{
    public static class ServerPathCompletion
    {
        public static IEnumerable<string> GetCompletions(HttpState programState, string normalCompletionString)
        {
            //If it's an absolute URI, nothing to suggest
            if (Uri.IsWellFormedUriString(normalCompletionString, UriKind.Absolute))
            {
                return null;
            }

            programState = programState ?? throw new ArgumentNullException(nameof(programState));

            normalCompletionString = normalCompletionString ?? throw new ArgumentNullException(nameof(normalCompletionString));

            if (programState.Structure is null)
            {
                return null;
            }

            string path = normalCompletionString.Replace('\\', '/');
            int searchFrom = normalCompletionString.Length - 1;
            int lastSlash = path.LastIndexOf('/', searchFrom);
            string prefix;

            if (lastSlash < 0)
            {
                path = string.Empty;
                prefix = normalCompletionString;
            }
            else
            {
                path = path.Substring(0, lastSlash + 1);
                prefix = normalCompletionString.Substring(lastSlash + 1);
            }

            IDirectoryStructure s = programState.Structure.TraverseTo(programState.PathSections.Reverse()).TraverseTo(path);

            if (s?.DirectoryNames == null)
            {
                return null;
            }

            List<string> results = new List<string>();

            foreach (string child in s.DirectoryNames)
            {
                if (child.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(path + child);
                }
            }

            return results;
        }
    }
}
