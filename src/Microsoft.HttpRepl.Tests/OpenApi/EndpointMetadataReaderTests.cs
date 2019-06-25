using System.Collections.Generic;
using System.Linq;
using Microsoft.HttpRepl.OpenApi;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.HttpRepl.Tests.OpenApi
{
    public class EndpointMetadataReaderTests
    {
        [Fact]
        public void Read_WithInvalidJson_ReturnsNull()
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
        public void Read_WithSwaggerV2InputAndNoPaths_ReturnsEmptyListOfEndPointMetaData()
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
        public void Read_WithSwaggerV2InputAndNoProperties_ReturnsEmptyListOfEndPointMetaData()
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
        public void Read_WithSwaggerV2Input_ReturnsEndpointMetadata()
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
  },
  ""definitions"": {
    ""Employee"": {
      ""type"": ""object"",
      ""properties"": {
        ""id"": {
          ""format"": ""int32"",
          ""type"": ""integer""
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
        public void Read_WithOpenApiV3InputAndNoPaths_ReturnsEmptyListOfEndPointMetaData()
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
        public void Read_WithOpenApiV3InputAndNoProperties_ReturnsEmptyListOfEndPointMetaData()
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
        public void Read_WithOpenApiV3InputWithNoRequestBody_ReturnsEndpointMetadataWithEmptyAvailableRequests()
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
        public void Read_WithOpenApiV3InputWithNoContent_ReturnsEndpointMetadataWithEmptyAvailableRequests()
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
        public void Read_WithOpenApiV3Input_ReturnsEndpointMetadata()
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

        private List<EndpointMetadata> GetEndpointMetadataList(string json)
        {
            JObject jobject = JObject.Parse(json);
            EndpointMetadataReader endpointMetadataReader = new EndpointMetadataReader();

            return endpointMetadataReader.Read(jobject).ToList();
        }
    }
}
