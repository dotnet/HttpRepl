using System;
using Microsoft.HttpRepl.OpenApi;
using Newtonsoft.Json.Linq;

namespace Microsoft.HttpRepl.Fakes
{
    public class EndPointMetaDataReaderStub : IEndpointMetadataReader
    {
        private ApiDefinition _apiDefinition;

        public EndPointMetaDataReaderStub(ApiDefinition apiDefinition)
        {
            _apiDefinition = apiDefinition;
        }

        public bool CanHandle(JObject document)
        {
            return (document["fakeApi"]?.ToString() ?? "").StartsWith("1.", StringComparison.Ordinal);
        }

        public ApiDefinition ReadMetadata(JObject document, Uri swaggerUri)
        {
            return _apiDefinition;
        }
    }
}
