// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.HttpRepl.Fakes;
using Microsoft.HttpRepl.OpenApi;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.HttpRepl.Tests.OpenApi
{
    public class EndpointMetadataReaderTests
    {
        [Fact]
        public void Read_WithJObjectFormatNotSupportedByAnyExistingReader_ReturnsNull()
        {
            string json = @"{
  ""info"": {
    ""version"": ""v1"",
    ""title"": ""My API""
  }
}";
            JObject jobject = JObject.Parse(json);
            EndpointMetadataReader endpointMetadataReader = new EndpointMetadataReader();

            Assert.Null(endpointMetadataReader.Read(jobject));
        }

        [Fact]
        public void RegisterReader_AddNewReader_VerifyReadReturnsEndpointMetadataCollection()
        {
            string json = @"{
  ""fakeApi"": ""1.0.0"",
  ""info"": {
    ""version"": ""v1""
  }
}";
            JObject jobject = JObject.Parse(json);
            EndpointMetadata endpointMetadata = new EndpointMetadata(path: "/api/Employees",
                requestsByMethodAndContentType: new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyList<Parameter>>>());
            EndPointMetaDataReaderStub endPointMetaDataReaderStub = new EndPointMetaDataReaderStub(endpointMetadata);

            EndpointMetadataReader endpointMetadataReader = new EndpointMetadataReader();
            endpointMetadataReader.RegisterReader(endPointMetaDataReaderStub);

            IEnumerable<EndpointMetadata> result = endpointMetadataReader.Read(jobject);

            Assert.Single(result);
            Assert.Equal(endpointMetadata, result.First());
        }
    }
}
