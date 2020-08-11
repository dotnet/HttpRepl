// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.HttpRepl.OpenApi;
using Newtonsoft.Json;
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

        public bool CanHandle(string document)
        {
            JObject doc;
            using (StringReader stringReader = new StringReader(document))
            {
                JsonSerializer serializer = new JsonSerializer();
                doc = (JObject)serializer.Deserialize(stringReader, typeof(JObject));
            }

            return (doc["fakeApi"]?.ToString() ?? "").StartsWith("1.", StringComparison.Ordinal);
        }

        public ApiDefinition ReadDefinition(string document, Uri sourceUri)
        {
            return _apiDefinition;
        }
    }
}
