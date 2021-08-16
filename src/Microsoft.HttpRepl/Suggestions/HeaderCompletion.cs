// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.HttpRepl.Suggestions
{
    public static class HeaderCompletion
    {
        /// <summary>
        /// Gets a collection of HTTP header names which starts with the prefix value passed in, except the ones present in existingHeaders.
        /// </summary>
        /// <param name="existingHeaders">A collection of existing headers.</param>
        /// <param name="prefix">The prefix value to get completion suggestion items for.</param>
        /// <returns></returns>
        public static IEnumerable<string> GetCompletions(IReadOnlyCollection<string> existingHeaders, string prefix)
        {
            return WellKnownHeaders.CommonHeaders.Where(x => x.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
                                                             existingHeaders?.Contains(x) != true);
        }

        public static IEnumerable<string> GetValueCompletions(string method, string path, string header, string prefix, HttpState programState)
        {
            header = header ?? throw new ArgumentNullException(nameof(header));

            programState = programState ?? throw new ArgumentNullException(nameof(programState));

            switch (header.ToUpperInvariant())
            {
                case "CONTENT-TYPE":
                    IEnumerable<string> results = programState.GetApplicableContentTypes(method, path);

                    return results?.Where(x => !string.IsNullOrEmpty(x) && x.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
                default:
                    return null;
            }
        }
    }
}
