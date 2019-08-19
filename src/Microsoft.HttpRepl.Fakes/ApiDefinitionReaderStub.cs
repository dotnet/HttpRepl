// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.HttpRepl.OpenApi;
using Newtonsoft.Json.Linq;

namespace Microsoft.HttpRepl.Fakes
{
    public class ApiDefinitionReaderStub : IApiDefinitionReader
    {
        private ApiDefinition _apiDefinition;

        public ApiDefinitionReaderStub(ApiDefinition apiDefinition)
        {
            _apiDefinition = apiDefinition;
        }

        public bool CanHandle(JObject document)
        {
            return (document["fakeApi"]?.ToString() ?? "").StartsWith("1.", StringComparison.Ordinal);
        }

        public ApiDefinition ReadDefinition(JObject document, Uri swsourceUri)
        {
            return _apiDefinition;
        }
    }
}
