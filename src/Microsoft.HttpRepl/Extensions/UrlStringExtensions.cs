// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;

namespace Microsoft.HttpRepl
{
    public static class UrlStringExtensions
    {
        public static string EnsureTrailingSlash(this string url)
        {
            url = url ?? throw new ArgumentNullException(nameof(url));

            if (!url.EndsWith("/", StringComparison.Ordinal))
            {
                url += "/";
            }

            return url;
        }
    }
}
