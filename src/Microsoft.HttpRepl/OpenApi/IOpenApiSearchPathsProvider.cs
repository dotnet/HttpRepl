// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

#nullable enable

using System.Collections.Generic;

namespace Microsoft.HttpRepl.OpenApi
{
    internal interface IOpenApiSearchPathsProvider
    {
        IEnumerable<string> GetOpenApiSearchPaths();
    }
}
