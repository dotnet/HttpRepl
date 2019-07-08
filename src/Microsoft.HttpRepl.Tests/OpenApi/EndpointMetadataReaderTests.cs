// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
    }
}
