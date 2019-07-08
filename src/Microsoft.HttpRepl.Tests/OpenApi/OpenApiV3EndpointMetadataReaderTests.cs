// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.HttpRepl.OpenApi;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.HttpRepl.Tests.OpenApi
{
    public class OpenApiV3EndpointMetadataReaderTests
    {
        [Fact]
        public void ReadMetadata_WithNoPaths_ReturnsEmptyListOfEndPointMetaData()
        {
            string json = @"{
  ""openapi"": ""3.0.0"",
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
  ""openapi"": ""3.0.0"",
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
        public void ReadMetadata_WithNoRequestBody_ReturnsEndpointMetadataWithEmptyAvailableRequests()
        {
            string json = @"{
  ""openapi"": ""3.0.0"",
  ""paths"": {
    ""/pets"": {
      ""post"": {
        ""summary"": ""Create a pet"",
        ""operationId"": ""createPets"",
        ""responses"": {
          ""201"": {
            ""description"": ""Null response""
          }
        }
      }
    }
  }
}";
            List<EndpointMetadata> endpointMetadata = GetEndpointMetadataList(json);

            Assert.Single(endpointMetadata);
            Assert.Equal("/pets", endpointMetadata[0].Path);
            Assert.Empty(endpointMetadata[0].AvailableRequests);
        }

        [Fact]
        public void ReadMetadata_WithNoContent_ReturnsEndpointMetadataWithEmptyAvailableRequests()
        {
            string json = @"{
  ""openapi"": ""3.0.0"",
  ""paths"": {
    ""/pets"": {
      ""post"": {
        ""requestBody"": {
          ""summary"": ""Create a pet"",
          ""operationId"": ""createPets"",
          ""responses"": {
            ""201"": {
              ""description"": ""Null response""
            }
          }
        }
      }
    }
  }
}";
            List<EndpointMetadata> endpointMetadata = GetEndpointMetadataList(json);

            Assert.Single(endpointMetadata);
            Assert.Equal("/pets", endpointMetadata[0].Path);
            Assert.Empty(endpointMetadata[0].AvailableRequests);
        }

        [Fact]
        public void Read_WithValidInput_ReturnsEndpointMetadata()
        {
            string json = @"{
  ""openapi"": ""3.0.0"",
  ""paths"": {
    ""/pets"": {
      ""get"": {
        ""requestBody"": {
          ""content"": {
            ""summary"": ""List all pets"",
            ""operationId"": ""listPets"",
            ""parameters"": [
              {
                ""name"": ""limit"",
                ""in"": ""query"",
                ""required"": false,
                ""schema"": {
                  ""type"": ""integer"",
                  ""format"": ""int32""
                }
              }
            ],
            ""responses"": {
              ""200"": {
                ""description"": ""An paged array of pets""
              }
            }
          }
        }
      },
      ""post"": {
        ""requestBody"": {
          ""content"": {
            ""summary"": ""Create a pet"",
            ""operationId"": ""createPets"",
            ""responses"": {
              ""201"": {
                ""description"": ""Null response""
              }
            }
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
            Assert.Equal("/pets", endpointMetadata[0].Path);

            Assert.Equal(2, availableRequests.Count);
            Assert.Equal("get", firstRequest.Key);
            Assert.Equal("post", secondRequest.Key);
        }

        [Fact]
        public void CanHandle_WithNoOpenApiKeyInDocument_ReturnsFalse()
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
        public void CanHandle_WithValidOpenApiVersionInDocument_ReturnsTrue()
        {
            string json = @"{
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""version"": ""v1""
  },
   ""paths"": {
  }
}";
            Assert.True(CanHandle(json));
        }

        [Fact]
        public void CanHandle_WithOpenApiVersionGreaterThanThree_ReturnsFalse()
        {
            string json = @"{
  ""openapi"": ""4.0.0"",
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
            OpenApiV3EndpointMetadataReader openApiV3EndpointMetadataReader = new OpenApiV3EndpointMetadataReader();

            return openApiV3EndpointMetadataReader.ReadMetadata(jobject).ToList();
        }

        private bool CanHandle(string json)
        {
            JObject jobject = JObject.Parse(json);
            OpenApiV3EndpointMetadataReader openApiV3EndpointMetadataReader = new OpenApiV3EndpointMetadataReader();

            return openApiV3EndpointMetadataReader.CanHandle(jobject);
        }
    }
}
