using System;
using System.Collections.Generic;
using Microsoft.HttpRepl.OpenApi;
using Newtonsoft.Json.Linq;

namespace Microsoft.HttpRepl.Tests.Fakes
{
    internal class EndPointMetaDataReaderStub : IEndpointMetadataReader
    {
        private EndpointMetadata _endpointMetadata;

        public EndPointMetaDataReaderStub(EndpointMetadata endpointMetadata)
        {
            _endpointMetadata = endpointMetadata;
        }

        public bool CanHandle(JObject document)
        {
            return (document["fakeApi"]?.ToString() ?? "").StartsWith("1.", StringComparison.Ordinal);
        }

        public IEnumerable<EndpointMetadata> ReadMetadata(JObject document)
        {
            return new List<EndpointMetadata> { _endpointMetadata };
        }
    }
}
