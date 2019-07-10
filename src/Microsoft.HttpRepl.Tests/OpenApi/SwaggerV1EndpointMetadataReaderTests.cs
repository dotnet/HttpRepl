// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.HttpRepl.OpenApi;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.HttpRepl.Tests.OpenApi
{
    public class SwaggerV1EndpointMetadataReaderTests
    {
        [Fact]
        public void ReadMetadata_WithNoApis_ReturnsEmptyListOfEndPointMetaData()
        {
            string json = @"{
  ""swaggerVersion"": ""1.0.0"",
  ""info"": {
    ""version"": ""v1""
  }
}";
            JObject jobject = JObject.Parse(json);
            SwaggerV1EndpointMetadataReader swaggerV1EndpointMetadataReader = new SwaggerV1EndpointMetadataReader();

            List<EndpointMetadata> endpointMetadata = swaggerV1EndpointMetadataReader.ReadMetadata(jobject).ToList();

            Assert.Empty(endpointMetadata);
        }

        [Fact]
        public void ReadMetadata_WithNoPath_ReturnsEmptyListOfEndPointMetaData()
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

            List<EndpointMetadata> endpointMetadata = swaggerV1EndpointMetadataReader.ReadMetadata(jobject).ToList();

            Assert.Empty(endpointMetadata);
        }

        [Fact]
        public void ReadMetadata_WithNoMethods_ReturnsEndPointMetadataWithNoAvailableRequests()
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

            List<EndpointMetadata> endpointMetadata = swaggerV1EndpointMetadataReader.ReadMetadata(jobject).ToList();

            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<Parameter>>> availableRequests = endpointMetadata[0].AvailableRequests;

            Assert.Single(endpointMetadata);
            Assert.Equal("/user/logout", endpointMetadata[0].Path);
            Assert.Empty(availableRequests);
        }

        [Fact]
        public void ReadMetadata_WithNoOperations_ReturnsEndPointMetadataWithNoAvailableRequests()
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

            List<EndpointMetadata> endpointMetadata = swaggerV1EndpointMetadataReader.ReadMetadata(jobject).ToList();

            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<Parameter>>> availableRequests = endpointMetadata[0].AvailableRequests;

            Assert.Single(endpointMetadata);
            Assert.Equal("/user/logout", endpointMetadata[0].Path);
            Assert.Empty(availableRequests);
        }

        [Fact]
        public void ReadMetadata_WithSingleObjectInApisArray_ReturnsEndpointMetadata()
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

            List<EndpointMetadata> endpointMetadata = swaggerV1EndpointMetadataReader.ReadMetadata(jobject).ToList();

            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<Parameter>>> availableRequests = endpointMetadata[0].AvailableRequests;

            Assert.Single(endpointMetadata);
            Assert.Equal("/user", endpointMetadata[0].Path);

            Assert.Equal(2, availableRequests.Count);
            Assert.True(availableRequests.ContainsKey("PUT"));
            Assert.True(availableRequests.ContainsKey("DELETE"));
        }

        [Fact]
        public void ReadMetadata_WithMultipleObjectsInApisArray_ReturnsEndpointMetadata()
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

            List<EndpointMetadata> endpointMetadata = swaggerV1EndpointMetadataReader.ReadMetadata(jobject).ToList();

            EndpointMetadata endpointMetadata1 = endpointMetadata[0];
            EndpointMetadata endpointMetadata2 = endpointMetadata[1];

            Assert.Equal(2, endpointMetadata.Count);

            Assert.Equal("/user/logout", endpointMetadata1.Path);
            Assert.Single(endpointMetadata1.AvailableRequests);
            Assert.Equal("GET", endpointMetadata1.AvailableRequests.First().Key);

            Assert.Equal("/user/login", endpointMetadata2.Path);
            Assert.Single(endpointMetadata2.AvailableRequests);
            Assert.Equal("GET", endpointMetadata2.AvailableRequests.First().Key);
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
