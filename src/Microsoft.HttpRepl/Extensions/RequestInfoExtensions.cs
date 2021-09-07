// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.HttpRepl
{
    public static class RequestInfoExtensions
    {
        public static string GetDirectoryMethodListing(this IRequestInfo requestInfo)
        {
            if (requestInfo is null || requestInfo.Methods is null || requestInfo.Methods.Count == 0)
            {
                return "[]";
            }

            IEnumerable<string> upperCaseMethods = requestInfo.Methods.Select(s => s?.ToUpperInvariant());

            return "[" + string.Join("|", upperCaseMethods) + "]";
        }
    }
}
