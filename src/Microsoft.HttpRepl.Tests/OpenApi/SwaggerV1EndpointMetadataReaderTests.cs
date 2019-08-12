// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.HttpRepl.OpenApi;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.HttpRepl.Tests.OpenApi
{
    public class SwaggerV1EndpointMetadataReaderTests
    {
        [Fact]
        public void ReadMetadata_WithNoApis_ReturnsApiDefinitionWithNoDirectories()
        {
            string json = @"{
  ""swaggerVersion"": ""1.0.0"",
  ""info"": {
    ""version"": ""v1""
  }
}";
            JObject jobject = JObject.Parse(json);
            SwaggerV1EndpointMetadataReader swaggerV1EndpointMetadataReader = new SwaggerV1EndpointMetadataReader();

            ApiDefinition apiDefinition = swaggerV1EndpointMetadataReader.ReadMetadata(jobject, null);

            Assert.NotNull(apiDefinition?.DirectoryStructure);
            Assert.Empty(apiDefinition.DirectoryStructure.DirectoryNames);
        }

        [Fact]
        public void ReadMetadata_WithNoPath_ReturnsApiDefinitionWithNoDirectories()
        {
            string json = @"{
  ""swaggerVersion"": ""1.0.0"",
  ""apis"": [
    {
        ""description"": ""resource1""
    }]
}";
            JObject jobject = JObject.Parse(json);
            SwaggerV1EndpointMetadataReader swaggerV1EndpointMetadataReader = new SwaggerV1EndpointMetadataReader();

            ApiDefinition apiDefinition = swaggerV1EndpointMetadataReader.ReadMetadata(jobject, null);

            Assert.NotNull(apiDefinition?.DirectoryStructure);
            Assert.Empty(apiDefinition.DirectoryStructure.DirectoryNames);
        }

        [Fact]
        public void ReadMetadata_WithNoMethods_ReturnsApiDefinitionWithNullRequestInfo()
        {
            string json = @"{
  ""swaggerVersion"": ""1.2"",
  ""apis"": [
    {
      ""path"": ""/user/logout"",
      ""operations"": [
        {
        }
      ]
    }
  ]
}";
            JObject jobject = JObject.Parse(json);
            SwaggerV1EndpointMetadataReader swaggerV1EndpointMetadataReader = new SwaggerV1EndpointMetadataReader();

            ApiDefinition apiDefinition = swaggerV1EndpointMetadataReader.ReadMetadata(jobject, null);

            Assert.NotNull(apiDefinition?.DirectoryStructure);

            IDirectoryStructure subDirectory = apiDefinition.DirectoryStructure.TraverseTo("/user/logout");

            Assert.Null(subDirectory.RequestInfo);
        }

        [Fact]
        public void ReadMetadata_WithNoOperations_ReturnsApiDefinitionWithNullRequestInfo()
        {
            string json = @"{
  ""swaggerVersion"": ""1.2"",
  ""apis"": [
    {
      ""path"": ""/user/logout"",
    }
  ]
}";
            JObject jobject = JObject.Parse(json);
            SwaggerV1EndpointMetadataReader swaggerV1EndpointMetadataReader = new SwaggerV1EndpointMetadataReader();

            ApiDefinition apiDefinition = swaggerV1EndpointMetadataReader.ReadMetadata(jobject, null);

            Assert.NotNull(apiDefinition?.DirectoryStructure);

            IDirectoryStructure subDirectory = apiDefinition.DirectoryStructure.TraverseTo("/user/logout");

            Assert.Null(subDirectory.RequestInfo);
        }

        [Fact]
        public void ReadMetadata_WithSingleObjectInApisArray_ReturnsApiDefinition()
        {
            string json = @"{
  ""swaggerVersion"": ""1.2"",
  ""apis"": [
    {
      ""path"": ""/user"",
      ""operations"": [
        {
          ""method"": ""PUT"",
          ""parameters"": [
            {
              ""name"": ""username"",
              ""description"": ""name that need to be deleted"",
              ""required"": true,
              ""type"": ""string"",
              ""paramType"": ""path""
            }
          ]
        },
        {
          ""method"": ""DELETE"",
          ""summary"": ""Delete user"",
          ""notes"": ""This can only be done by the logged in user."",
          ""type"": ""void"",
          ""nickname"": ""deleteUser"",
          ""parameters"": [
            {
              ""name"": ""username"",
              ""description"": ""The name that needs to be deleted"",
              ""required"": true,
              ""type"": ""string"",
              ""paramType"": ""path""
            }
          ]
        }
      ]
    }
  ],
  ""models"": {
    ""User"": {
      ""id"": ""User"",
      ""properties"": {
        ""id"": {
          ""type"": ""integer"",
          ""format"": ""int64""
        },
        ""firstName"": {
          ""type"": ""string""
        }
      }
    }
  }
}";
            JObject jobject = JObject.Parse(json);
            SwaggerV1EndpointMetadataReader swaggerV1EndpointMetadataReader = new SwaggerV1EndpointMetadataReader();

            ApiDefinition apiDefinition = swaggerV1EndpointMetadataReader.ReadMetadata(jobject, null);

            Assert.NotNull(apiDefinition?.DirectoryStructure);
            Assert.Single(apiDefinition.DirectoryStructure.DirectoryNames);
            Assert.Equal("user", apiDefinition.DirectoryStructure.DirectoryNames.Single());

            IDirectoryStructure subDirectory = apiDefinition.DirectoryStructure.TraverseTo("/user");

            Assert.Equal(2, subDirectory.RequestInfo.Methods.Count);
            Assert.Contains("PUT", subDirectory.RequestInfo.Methods, StringComparer.Ordinal);
            Assert.Contains("DELETE", subDirectory.RequestInfo.Methods, StringComparer.Ordinal);
        }

        [Fact]
        public void ReadMetadata_WithMultipleObjectsInApisArray_ReturnsApiDefinition()
        {
            string json = @"{
  ""swaggerVersion"": ""1.2"",
  ""apis"": [
    {
      ""path"": ""/user/logout"",
      ""operations"": [
        {
          ""method"": ""GET"",
          ""parameters"": []
        }
      ]
    },
    {
      ""path"": ""/user/login"",
      ""operations"": [
        {
          ""method"": ""GET"",
          ""parameters"": [
            {
              ""name"": ""username"",
              ""description"": ""The user name for login"",
              ""required"": true,
              ""type"": ""string"",
              ""paramType"": ""query""
            }
          ]
        }
      ]
    }
  ],
  ""models"": {
    ""User"": {
      ""id"": ""User"",
      ""properties"": {
        ""id"": {
          ""type"": ""integer"",
          ""format"": ""int64""
        },
        ""firstName"": {
          ""type"": ""string""
        }
      }
    }
  }
}";
            JObject jobject = JObject.Parse(json);
            SwaggerV1EndpointMetadataReader swaggerV1EndpointMetadataReader = new SwaggerV1EndpointMetadataReader();

            ApiDefinition apiDefinition = swaggerV1EndpointMetadataReader.ReadMetadata(jobject, null);

            Assert.NotNull(apiDefinition?.DirectoryStructure);
            Assert.Single(apiDefinition.DirectoryStructure.DirectoryNames);
            Assert.Equal("user", apiDefinition.DirectoryStructure.DirectoryNames.Single());

            IDirectoryStructure logoutSubDirectory = apiDefinition.DirectoryStructure.TraverseTo("/user/logout");

            Assert.Single(logoutSubDirectory.RequestInfo.Methods);
            Assert.Contains("GET", logoutSubDirectory.RequestInfo.Methods, StringComparer.Ordinal);

            IDirectoryStructure loginSubDirectory = apiDefinition.DirectoryStructure.TraverseTo("/user/logout");

            Assert.Single(loginSubDirectory.RequestInfo.Methods);
            Assert.Contains("GET", loginSubDirectory.RequestInfo.Methods, StringComparer.Ordinal);
        }

        [Fact]
        public void CanHandle_WithNoSwaggerVersionKeyInDocument_ReturnsFalse()
        {
            string json = @"{
  ""apis"": [
    {
      ""path"": ""/user/logout"",
      ""operations"": [
        {
        }
      ]
    }
  ]
}";
            JObject jobject = JObject.Parse(json);
            SwaggerV1EndpointMetadataReader swaggerV1EndpointMetadataReader = new SwaggerV1EndpointMetadataReader();

            bool? result = swaggerV1EndpointMetadataReader.CanHandle(jobject);

            Assert.False(result);
        }

        [Fact]
        public void CanHandle_WithValidSwaggerVersionKeyInDocument_ReturnsTrue()
        {
            string json = @"{
  ""swaggerVersion"": ""1.2"",
  ""apis"": [
    {
      ""path"": ""/user/logout"",
      ""operations"": [
        {
        }
      ]
    }
  ]
}";
            JObject jobject = JObject.Parse(json);
            SwaggerV1EndpointMetadataReader swaggerV1EndpointMetadataReader = new SwaggerV1EndpointMetadataReader();

            bool? result = swaggerV1EndpointMetadataReader.CanHandle(jobject);

            Assert.True(result);
        }

        [Fact]
        public void CanHandle_WithSwaggerVersionGreaterThanOne_ReturnsFalse()
        {
            string json = @"{
  ""swaggerVersion"": ""2.2"",
  ""apis"": [
    {
      ""path"": ""/user/logout"",
      ""operations"": [
        {
        }
      ]
    }
  ]
}";
            JObject jobject = JObject.Parse(json);
            SwaggerV1EndpointMetadataReader swaggerV1EndpointMetadataReader = new SwaggerV1EndpointMetadataReader();

            bool? result = swaggerV1EndpointMetadataReader.CanHandle(jobject);

            Assert.False(result);
        }
    }
}
