// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System.Collections.Generic;

namespace Microsoft.HttpRepl.OpenApi
{
    internal interface IOpenApiSearchPathsProvider
    {
        IEnumerable<string> GetOpenApiSearchPaths();
    }
}
