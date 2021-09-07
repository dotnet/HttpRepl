// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

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

        public ApiDefinitionParseResult CanHandle(string document)
        {
            JObject doc;
            using (StringReader stringReader = new StringReader(document))
            {
                JsonSerializer serializer = new JsonSerializer();
                doc = (JObject)serializer.Deserialize(stringReader, typeof(JObject));
            }

            return (doc["fakeApi"]?.ToString() ?? "").StartsWith("1.", StringComparison.Ordinal) ? new ApiDefinitionParseResult(true, null, null) : ApiDefinitionParseResult.Failed;
        }

        public ApiDefinitionParseResult ReadDefinition(string document, Uri sourceUri)
        {
            return new ApiDefinitionParseResult(true, _apiDefinition, null);
        }
    }
}
