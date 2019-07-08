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
            List<EndpointMetadata> endpointMetadata = GetEndpointMetadataList(json);

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
            List<EndpointMetadata> endpointMetadata = GetEndpointMetadataList(json);

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
            List<EndpointMetadata> endpointMetadata = GetEndpointMetadataList(json);
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
            List<EndpointMetadata> endpointMetadata = GetEndpointMetadataList(json);
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<Parameter>>> availableRequests = endpointMetadata[0].AvailableRequests;
            KeyValuePair<string, IReadOnlyDictionary<string, IReadOnlyList<Parameter>>> firstRequest = availableRequests.First();
            KeyValuePair<string, IReadOnlyDictionary<string, IReadOnlyList<Parameter>>> secondRequest = availableRequests.ElementAt(1);

            Assert.Single(endpointMetadata);
            Assert.Equal("/api/Employees", endpointMetadata[0].Path);

            Assert.Equal(2, availableRequests.Count);
            Assert.Equal("get", firstRequest.Key);
            Assert.Equal("post", secondRequest.Key);
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
            Assert.False(CanHandle(json));
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
            Assert.True(CanHandle(json));
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
            Assert.False(CanHandle(json));
        }

        private List<EndpointMetadata> GetEndpointMetadataList(string json)
        {
            JObject jobject = JObject.Parse(json);
            SwaggerV2EndpointMetadataReader swaggerV2EndpointMetadataReader = new SwaggerV2EndpointMetadataReader();

            return swaggerV2EndpointMetadataReader.ReadMetadata(jobject).ToList();
        }

        private bool CanHandle(string json)
        {
            JObject jobject = JObject.Parse(json);
            SwaggerV2EndpointMetadataReader swaggerV2EndpointMetadataReader = new SwaggerV2EndpointMetadataReader();

            return swaggerV2EndpointMetadataReader.CanHandle(jobject);
        }
    }
}
