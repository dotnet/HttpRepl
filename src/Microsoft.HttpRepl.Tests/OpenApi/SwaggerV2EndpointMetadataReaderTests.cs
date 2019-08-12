// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.HttpRepl.OpenApi;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.HttpRepl.Tests.OpenApi
{
    public class SwaggerV2EndpointMetadataReaderTests
    {
        [Fact]
        public void ReadMetadata_WithNoPaths_ReturnsApiDefinitionWithNoDirectories()
        {
            string json = @"{
  ""swagger"": ""2.0"",
  ""info"": {
    ""version"": ""v1""
  }
}";
            JObject jobject = JObject.Parse(json);
            SwaggerV2EndpointMetadataReader swaggerV2EndpointMetadataReader = new SwaggerV2EndpointMetadataReader();

            ApiDefinition apiDefinition = swaggerV2EndpointMetadataReader.ReadMetadata(jobject, null);

            Assert.NotNull(apiDefinition?.DirectoryStructure);
            Assert.Empty(apiDefinition.DirectoryStructure.DirectoryNames);
        }

        [Fact]
        public void ReadMetadata_WithNoProperties_ReturnsApiDefinitionWithNoDirectories()
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

            ApiDefinition apiDefinition = swaggerV2EndpointMetadataReader.ReadMetadata(jobject, null);

            Assert.NotNull(apiDefinition?.DirectoryStructure);
            Assert.Empty(apiDefinition.DirectoryStructure.DirectoryNames);
        }

        [Fact]
        public void ReadMetadata_WithNoRequestMethods_ReturnsEndpointMetadataWithNullRequestInfo()
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

            ApiDefinition apiDefinition = swaggerV2EndpointMetadataReader.ReadMetadata(jobject, null);

            Assert.NotNull(apiDefinition?.DirectoryStructure);

            IDirectoryStructure subDirectory = apiDefinition.DirectoryStructure.TraverseTo("/api/Employees");

            Assert.Null(subDirectory.RequestInfo);
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

            ApiDefinition apiDefinition = swaggerV2EndpointMetadataReader.ReadMetadata(jobject, null);

            Assert.NotNull(apiDefinition?.DirectoryStructure);
            Assert.Single(apiDefinition.DirectoryStructure.DirectoryNames);
            Assert.Equal("api", apiDefinition.DirectoryStructure.DirectoryNames.Single());

            IDirectoryStructure subDirectory = apiDefinition.DirectoryStructure.TraverseTo("/api/Employees");

            Assert.Equal(2, subDirectory.RequestInfo.Methods.Count);
            Assert.Contains("get", subDirectory.RequestInfo.Methods, StringComparer.Ordinal);
            Assert.Contains("post", subDirectory.RequestInfo.Methods, StringComparer.Ordinal);
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
