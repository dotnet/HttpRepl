// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

#nullable enable

using System;

namespace Microsoft.HttpRepl.OpenApi
{
    public interface IApiDefinitionReader
    {
        ApiDefinitionParseResult CanHandle(string document);

        ApiDefinitionParseResult ReadDefinition(string document, Uri? sourceUri);
    }
}
