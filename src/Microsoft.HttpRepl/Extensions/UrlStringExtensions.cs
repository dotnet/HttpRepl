using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.HttpRepl
{
    public static class UrlStringExtensions
    {
        public static string EnsureTrailingSlash(this string url)
        {
            if (!url.EndsWith("/", StringComparison.Ordinal))
            {
                url += "/";
            }

            return url;
        }
    }
}
