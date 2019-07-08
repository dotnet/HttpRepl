// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
        public void Read_WithSwaggerV2InputAndNoRequestMethods_ReturnsEndpointMetadataWithEmptyAvailableRequests()
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
        public void Read_WithOpenApiV3InputAndNoRequestBody_ReturnsEndpointMetadataWithEmptyAvailableRequests()
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
        public void Read_WithOpenApiV3InputAndNoContent_ReturnsEndpointMetadataWithEmptyAvailableRequests()
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

        [Fact]
        public void Read_WithSwaggerV1InputAndNoApis_ReturnsEmptyListOfEndPointMetaData()
        {
            string json = @"{
  ""swaggerVersion"": ""1.0.0"",
  ""info"": {
    ""version"": ""v1""
  }
}";
            List<EndpointMetadata> endpointMetadata = GetEndpointMetadataList(json);

            Assert.Empty(endpointMetadata);
        }

        [Fact]
        public void Read_WithSwaggerV1InputAndNoPath_ReturnsEmptyListOfEndPointMetaData()
        {
            string json = @"{
  ""swaggerVersion"": ""1.0.0"",
  ""apis"": [
    {
        ""description"": ""resource1""
    }]
}";
            List<EndpointMetadata> endpointMetadata = GetEndpointMetadataList(json);

            Assert.Empty(endpointMetadata);
        }

        [Fact]
        public void Read_WithSwaggerV1InputAndNoMethods_ReturnsEndPointMetadataWithNoAvailableRequests()
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
            List<EndpointMetadata> endpointMetadata = GetEndpointMetadataList(json);
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<Parameter>>> availableRequests = endpointMetadata[0].AvailableRequests;

            Assert.Single(endpointMetadata);
            Assert.Equal("/user/logout", endpointMetadata[0].Path);
            Assert.Empty(availableRequests);
        }

        [Fact]
        public void Read_WithSwaggerV1InputAndNoOperations_ReturnsEndPointMetadataWithNoAvailableRequests()
        {
            string json = @"{
  ""swaggerVersion"": ""1.2"",
  ""apis"": [
    {
      ""path"": ""/user/logout"",
    }
  ]
}";
            List<EndpointMetadata> endpointMetadata = GetEndpointMetadataList(json);
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<Parameter>>> availableRequests = endpointMetadata[0].AvailableRequests;

            Assert.Single(endpointMetadata);
            Assert.Equal("/user/logout", endpointMetadata[0].Path);
            Assert.Empty(availableRequests);
        }

        [Fact]
        public void Read_WithSwaggerV1InputAndSingleObjectInApisArray_ReturnsEndpointMetadata()
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
            List<EndpointMetadata> endpointMetadata = GetEndpointMetadataList(json);
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<Parameter>>> availableRequests = endpointMetadata[0].AvailableRequests;
            KeyValuePair<string, IReadOnlyDictionary<string, IReadOnlyList<Parameter>>> firstRequest = availableRequests.First();
            KeyValuePair<string, IReadOnlyDictionary<string, IReadOnlyList<Parameter>>> secondRequest = availableRequests.ElementAt(1);

            Assert.Single(endpointMetadata);
            Assert.Equal("/user", endpointMetadata[0].Path);

            Assert.Equal(2, availableRequests.Count);
            Assert.Equal("PUT", firstRequest.Key);
            Assert.Equal("DELETE", secondRequest.Key);
        }

        [Fact]
        public void Read_WithSwaggerV1InputAndMultipleObjectsInApisArray_ReturnsEndpointMetadata()
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
            List<EndpointMetadata> endpointMetadata = GetEndpointMetadataList(json);

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

        private List<EndpointMetadata> GetEndpointMetadataList(string json)
        {
            JObject jobject = JObject.Parse(json);
            EndpointMetadataReader endpointMetadataReader = new EndpointMetadataReader();

            return endpointMetadataReader.Read(jobject).ToList();
        }
    }
}
