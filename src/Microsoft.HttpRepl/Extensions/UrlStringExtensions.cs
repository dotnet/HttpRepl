// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
