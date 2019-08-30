// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
