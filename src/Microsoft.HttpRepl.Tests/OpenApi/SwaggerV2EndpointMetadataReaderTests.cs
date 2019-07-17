// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.HttpRepl.OpenApi;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.HttpRepl.Tests.OpenApi
{
    public class SwaggerV2EndpointMetadataReaderTests
    {
        [Fact]
        public void ReadMetadata_WithNoPaths_ReturnsEmptyListOfEndPointMetaData()
        {
            string json = @"{
  ""swagger"": ""2.0"",
  ""info"": {
    ""version"": ""v1""
  }
}";
            JObject jobject = JObject.Parse(json);
            SwaggerV2EndpointMetadataReader swaggerV2EndpointMetadataReader = new SwaggerV2EndpointMetadataReader();

            List<EndpointMetadata> endpointMetadata = swaggerV2EndpointMetadataReader.ReadMetadata(jobject).ToList();

            Assert.Empty(endpointMetadata);
        }

        [Fact]
        public void ReadMetadata_WithNoProperties_ReturnsEmptyListOfEndPointMetaData()
        {
            string json = @"{
  ""swagger"": ""2.0"",
  ""info"": {
    ""version"": ""v1""
  },
  ""paths"": {
  }
}";
            JObject jobject = JObject.Parse(json);
            SwaggerV2EndpointMetadataReader swaggerV2EndpointMetadataReader = new SwaggerV2EndpointMetadataReader();

            List<EndpointMetadata> endpointMetadata = swaggerV2EndpointMetadataReader.ReadMetadata(jobject).ToList();

            Assert.Empty(endpointMetadata);
        }

        [Fact]
        public void ReadMetadata_WithNoRequestMethods_ReturnsEndpointMetadataWithEmptyAvailableRequests()
        {
            string json = @"{
  ""swagger"": ""2.0"",
  ""info"": {
    ""version"": ""v1""
  },
  ""paths"": {
    ""/api/Employees"": {
    }
  }
}";
            JObject jobject = JObject.Parse(json);
            SwaggerV2EndpointMetadataReader swaggerV2EndpointMetadataReader = new SwaggerV2EndpointMetadataReader();

            List<EndpointMetadata> endpointMetadata = swaggerV2EndpointMetadataReader.ReadMetadata(jobject).ToList();
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<Parameter>>> availableRequests = endpointMetadata[0].AvailableRequests;

            Assert.Single(endpointMetadata);
            Assert.Equal("/api/Employees", endpointMetadata[0].Path);
            Assert.Empty(availableRequests);
        }

        [Fact]
        public void ReadMetadata_WithValidInput_ReturnsEndpointMetadata()
        {
            string json = @"{
  ""swagger"": ""2.0"",
  ""info"": {
    ""version"": ""v1""
  },
  ""paths"": {
    ""/api/Employees"": {
      ""get"": {
        ""tags"": [
          ""Employees""
        ],
        ""operationId"": ""GetEmployee"",
        ""consumes"": [],
        ""produces"": [
          ""text/plain""
        ],
        ""parameters"": [],
        ""responses"": {
          ""200"": {
            ""description"": ""Success""
          }
        }
      },
      ""post"": {
        ""tags"": [
          ""Employees""
        ],
        ""operationId"": ""put"",
        ""consumes"": [],
        ""produces"": [
          ""text/plain""
        ],
        ""parameters"": [
          {
            ""name"": ""id"",
            ""in"": ""path""
          }
        ],
        ""responses"": {
          ""200"": {
            ""description"": ""Success""
          }
        }
      }
    }
  }
}";
            JObject jobject = JObject.Parse(json);
            SwaggerV2EndpointMetadataReader swaggerV2EndpointMetadataReader = new SwaggerV2EndpointMetadataReader();

            List<EndpointMetadata> endpointMetadata = swaggerV2EndpointMetadataReader.ReadMetadata(jobject).ToList();
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<Parameter>>> availableRequests = endpointMetadata[0].AvailableRequests;

            Assert.Single(endpointMetadata);
            Assert.Equal("/api/Employees", endpointMetadata[0].Path);

            Assert.Equal(2, availableRequests.Count);
            Assert.True(availableRequests.ContainsKey("get"));
            Assert.True(availableRequests.ContainsKey("post"));
        }

        [Fact]
        public void CanHandle_WithNoSwaggerVersionKeyInDocument_ReturnsFalse()
        {
            string json = @"{
  ""info"": {
    ""version"": ""v1""
  },
  ""paths"": {
  }
}";
            JObject jobject = JObject.Parse(json);
            SwaggerV2EndpointMetadataReader swaggerV2EndpointMetadataReader = new SwaggerV2EndpointMetadataReader();

            bool? result = swaggerV2EndpointMetadataReader.CanHandle(jobject);

            Assert.False(result);
        }

        [Fact]
        public void CanHandle_WithValidSwaggerVersionKeyInDocument_ReturnsTrue()
        {
            string json = @"{
  ""swagger"": ""2.0"",
  ""info"": {
    ""version"": ""v1""
  },
  ""paths"": {
  }
}";
            JObject jobject = JObject.Parse(json);
            SwaggerV2EndpointMetadataReader swaggerV2EndpointMetadataReader = new SwaggerV2EndpointMetadataReader();

            bool? result = swaggerV2EndpointMetadataReader.CanHandle(jobject);

            Assert.True(result);
        }

        [Fact]
        public void CanHandle_WithSwaggerVersionGreaterThanTwo_ReturnsFalse()
        {
            string json = @"{
  ""swagger"": ""3.0"",
  ""info"": {
    ""version"": ""v1""
  },
  ""paths"": {
  }
}";
            JObject jobject = JObject.Parse(json);
            SwaggerV2EndpointMetadataReader swaggerV2EndpointMetadataReader = new SwaggerV2EndpointMetadataReader();

            bool? result = swaggerV2EndpointMetadataReader.CanHandle(jobject);

            Assert.False(result);
        }
    }
}
