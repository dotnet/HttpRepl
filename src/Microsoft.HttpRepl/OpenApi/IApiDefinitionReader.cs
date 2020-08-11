// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.HttpRepl.OpenApi
{
    public interface IApiDefinitionReader
    {
        bool CanHandle(string document);

        ApiDefinition ReadDefinition(string document, Uri sourceUri);
    }
}
