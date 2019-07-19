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
            JObject jobject = JObject.Parse(json);
            OpenApiV3EndpointMetadataReader openApiV3EndpointMetadataReader = new OpenApiV3EndpointMetadataReader();

            List<EndpointMetadata> endpointMetadata = openApiV3EndpointMetadataReader.ReadMetadata(jobject).ToList();

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
            JObject jobject = JObject.Parse(json);
            OpenApiV3EndpointMetadataReader openApiV3EndpointMetadataReader = new OpenApiV3EndpointMetadataReader();

            List<EndpointMetadata> endpointMetadata = openApiV3EndpointMetadataReader.ReadMetadata(jobject).ToList();

            Assert.Empty(endpointMetadata);
        }

        [Fact]
        public void ReadMetadata_WithNoResponses_ReturnsEndpointMetadataWithEmptyAvailableRequests()
        {
            string json = @"{
  ""openapi"": ""3.0.0"",
  ""paths"": {
    ""/pets"": {
      ""post"": {
        ""summary"": ""Create a pet"",
        ""operationId"": ""createPets"",
        ""requestBody"": {
          ""content"": {

          }
        }
      }
    }
  }
}";
            JObject jobject = JObject.Parse(json);
            OpenApiV3EndpointMetadataReader openApiV3EndpointMetadataReader = new OpenApiV3EndpointMetadataReader();

            List<EndpointMetadata> endpointMetadata = openApiV3EndpointMetadataReader.ReadMetadata(jobject).ToList();

            Assert.Single(endpointMetadata);
            Assert.Equal("/pets", endpointMetadata[0].Path);
            Assert.Empty(endpointMetadata[0].AvailableRequests);
        }

        [Fact]
        public void ReadMetadata_WithNoContent_ReturnsEndpointMetadataWithRequestButNoContentTypes()
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
        },
        ""requestBody"": {
          ""description"": ""A Request Body"",
          ""required"": false
        }
      }
    }
  }
}";
            JObject jobject = JObject.Parse(json);
            OpenApiV3EndpointMetadataReader openApiV3EndpointMetadataReader = new OpenApiV3EndpointMetadataReader();

            List<EndpointMetadata> endpointMetadata = openApiV3EndpointMetadataReader.ReadMetadata(jobject).ToList();

            Assert.Single(endpointMetadata);
            Assert.Equal("/pets", endpointMetadata[0].Path);
            Assert.Single(endpointMetadata[0].AvailableRequests);
            Assert.True(endpointMetadata[0].AvailableRequests.ContainsKey("post"));
            Assert.Empty(endpointMetadata[0].AvailableRequests["post"]);
        }

        [Fact]
        public void ReadMetadata_WithValidInput_ReturnsEndpointMetadata()
        {
            string json = @"{
  ""openapi"": ""3.0.0"",
  ""paths"": {
    ""/pets"": {
      ""get"": {
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
      },
      ""post"": {
        ""summary"": ""Create a pet"",
        ""operationId"": ""createPets"",
        ""responses"": {
          ""201"": {
            ""description"": ""Null response""
          }
        },
        ""requestBody"": {
          ""content"": {
            
          }
        }
      }
    }
  }
}";
            JObject jobject = JObject.Parse(json);
            OpenApiV3EndpointMetadataReader openApiV3EndpointMetadataReader = new OpenApiV3EndpointMetadataReader();

            List<EndpointMetadata> endpointMetadata = openApiV3EndpointMetadataReader.ReadMetadata(jobject).ToList();

            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<Parameter>>> availableRequests = endpointMetadata[0].AvailableRequests;

            Assert.Single(endpointMetadata);
            Assert.Equal("/pets", endpointMetadata[0].Path);

            Assert.Equal(2, availableRequests.Count);
            Assert.True(availableRequests.ContainsKey("get"));
            Assert.True(availableRequests.ContainsKey("post"));
        }

        [Fact]
        public void ReadMetadata_WithNoRequestBody_ReturnsEndpointMetadata()
        {
            string json = @"{
  ""openapi"": ""3.0.0"",
  ""paths"": {
    ""/pets"": {
      ""get"": {
        ""responses"": {
          ""200"": {
            ""description"": ""Success""
          }
        }
      },
      ""post"": {
        ""summary"": ""Create a pet"",
        ""operationId"": ""createPets"",
        ""responses"": {
          ""201"": {
            ""description"": ""Null response""
          }
        },
        ""requestBody"": {
          ""content"": {
            
          }
        }
      }
    }
  }
}";
            JObject jobject = JObject.Parse(json);
            OpenApiV3EndpointMetadataReader openApiV3EndpointMetadataReader = new OpenApiV3EndpointMetadataReader();

            List<EndpointMetadata> endpointMetadata = openApiV3EndpointMetadataReader.ReadMetadata(jobject).ToList();

            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<Parameter>>> availableRequests = endpointMetadata[0].AvailableRequests;

            Assert.Single(endpointMetadata);
            Assert.Equal("/pets", endpointMetadata[0].Path);

            Assert.Equal(2, availableRequests.Count);
            Assert.True(availableRequests.ContainsKey("get"));
            Assert.True(availableRequests.ContainsKey("post"));
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
            JObject jobject = JObject.Parse(json);
            OpenApiV3EndpointMetadataReader openApiV3EndpointMetadataReader = new OpenApiV3EndpointMetadataReader();

            bool? result = openApiV3EndpointMetadataReader.CanHandle(jobject);

            Assert.False(result);
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
            JObject jobject = JObject.Parse(json);
            OpenApiV3EndpointMetadataReader openApiV3EndpointMetadataReader = new OpenApiV3EndpointMetadataReader();

            bool? result = openApiV3EndpointMetadataReader.CanHandle(jobject);

            Assert.True(result);
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
            JObject jobject = JObject.Parse(json);
            OpenApiV3EndpointMetadataReader openApiV3EndpointMetadataReader = new OpenApiV3EndpointMetadataReader();

            bool? result = openApiV3EndpointMetadataReader.CanHandle(jobject);

            Assert.False(result);
        }
    }
}
