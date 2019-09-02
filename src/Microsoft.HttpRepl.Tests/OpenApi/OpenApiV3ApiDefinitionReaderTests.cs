// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.HttpRepl.OpenApi;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.HttpRepl.Tests.OpenApi
{
    public class OpenApiV3ApiDefinitionReaderTests
    {
        [Fact]
        public void ReadMetadata_WithNoPaths_ReturnsEmptyDirectoryStructure()
        {
            string json = @"{
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""version"": ""v1""
  }
}";
            JObject jobject = JObject.Parse(json);
            OpenApiV3ApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiV3ApiDefinitionReader();

            ApiDefinition apiDefinition = openApiV3ApiDefinitionReader.ReadDefinition(jobject, null);

            Assert.NotNull(apiDefinition?.DirectoryStructure);
            Assert.Empty(apiDefinition.DirectoryStructure.DirectoryNames);
        }

        [Fact]
        public void ReadMetadata_WithNoProperties_ReturnsNullDirectoryStructure()
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
            OpenApiV3ApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiV3ApiDefinitionReader();

            ApiDefinition apiDefinition = openApiV3ApiDefinitionReader.ReadDefinition(jobject, null);

            Assert.NotNull(apiDefinition?.DirectoryStructure);
            Assert.Empty(apiDefinition.DirectoryStructure.DirectoryNames);
        }

        [Fact]
        public void ReadMetadata_WithNoResponses_ReturnsApiDefinition()
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
            OpenApiV3ApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiV3ApiDefinitionReader();

            ApiDefinition apiDefinition = openApiV3ApiDefinitionReader.ReadDefinition(jobject, null);

            Assert.NotNull(apiDefinition?.DirectoryStructure);
            Assert.Single(apiDefinition.DirectoryStructure.DirectoryNames);
            Assert.Equal("pets", apiDefinition.DirectoryStructure.DirectoryNames.Single());

            IDirectoryStructure subDirectory = apiDefinition.DirectoryStructure.TraverseTo("/pets");

            Assert.Single(subDirectory.RequestInfo.Methods);
            Assert.Contains("post", subDirectory.RequestInfo.Methods, StringComparer.Ordinal);
        }

        [Fact]
        public void ReadMetadata_WithNoMethods_ReturnsApiDefinitionWithStructure()
        {
            string json = @"{
  ""openapi"": ""3.0.0"",
  ""paths"": {
    ""/pets"": {
    }
  }
}";
            JObject jobject = JObject.Parse(json);
            OpenApiV3ApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiV3ApiDefinitionReader();

            ApiDefinition apiDefinition = openApiV3ApiDefinitionReader.ReadDefinition(jobject, null);

            Assert.NotNull(apiDefinition?.DirectoryStructure);
        }

        [Fact]
        public void ReadMetadata_WithNoResponses_ReturnsApiDefinitionWithNoRequestInfo()
        {
            string json = @"{
  ""openapi"": ""3.0.0"",
  ""paths"": {
    ""/pets"": {
    }
  }
}";
            JObject jobject = JObject.Parse(json);
            OpenApiV3ApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiV3ApiDefinitionReader();

            ApiDefinition apiDefinition = openApiV3ApiDefinitionReader.ReadDefinition(jobject, null);

            IDirectoryStructure subDirectory = apiDefinition.DirectoryStructure.TraverseTo("/pets");

            Assert.Null(subDirectory.RequestInfo);
        }

        [Theory]
        [InlineData("get", true)]
        [InlineData("post", true)]
        [InlineData("put", true)]
        [InlineData("delete", true)]
        [InlineData("options", true)]
        [InlineData("head", true)]
        [InlineData("patch", true)]
        [InlineData("trace", true)]
        [InlineData("$ref", false)]
        [InlineData("summary", false)]
        [InlineData("description", false)]
        [InlineData("servers", false)]
        [InlineData("parameters", false)]
        [InlineData("", false)]
        public void ReadMetadata_WithSpecifiedMethodName_ReturnsApiDefinitionWithCorrectNumberOfRequestMethods(string method, bool shouldHaveRequest)
        {
            string json = @"{
  ""openapi"": ""3.0.0"",
  ""paths"": {
    ""/pets"": {
      """ + method + @""": """"
    }
  }
}";
            JObject jobject = JObject.Parse(json);
            OpenApiV3ApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiV3ApiDefinitionReader();

            ApiDefinition apiDefinition = openApiV3ApiDefinitionReader.ReadDefinition(jobject, null);

            Assert.NotNull(apiDefinition?.DirectoryStructure);
            Assert.Single(apiDefinition.DirectoryStructure.DirectoryNames);
            Assert.Equal("pets", apiDefinition.DirectoryStructure.DirectoryNames.Single());

            IDirectoryStructure subDirectory = apiDefinition.DirectoryStructure.TraverseTo("/pets");

            if (shouldHaveRequest)
            {
                Assert.Single(subDirectory.RequestInfo.Methods);
                Assert.Contains(method, subDirectory.RequestInfo.Methods, StringComparer.Ordinal);
            }
            else
            {
                Assert.Null(subDirectory.RequestInfo);
            }
        }

        [Fact]
        public void ReadMetadata_WithNoContent_ReturnsApiDefinitionWithRequestMethodButNoContentTypes()
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
            OpenApiV3ApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiV3ApiDefinitionReader();

            ApiDefinition apiDefinition = openApiV3ApiDefinitionReader.ReadDefinition(jobject, null);

            Assert.NotNull(apiDefinition?.DirectoryStructure);
            Assert.Single(apiDefinition.DirectoryStructure.DirectoryNames);
            Assert.Equal("pets", apiDefinition.DirectoryStructure.DirectoryNames.Single());

            IDirectoryStructure subDirectory = apiDefinition.DirectoryStructure.TraverseTo("/pets");

            Assert.Single(subDirectory.RequestInfo.Methods);
            Assert.Contains("post", subDirectory.RequestInfo.Methods, StringComparer.Ordinal);
            Assert.DoesNotContain("post", subDirectory.RequestInfo.ContentTypesByMethod.Keys, StringComparer.Ordinal);
        }

        [Fact]
        public void ReadMetadata_WithContentAndOneContentType_ReturnsApiDefinitionWithContentType()
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
          ""required"": false,
          ""content"": {
            ""application/json"": {
            }
          }
        }
      }
    }
  }
}";
            JObject jobject = JObject.Parse(json);
            OpenApiV3ApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiV3ApiDefinitionReader();

            ApiDefinition apiDefinition = openApiV3ApiDefinitionReader.ReadDefinition(jobject, null);

            Assert.NotNull(apiDefinition?.DirectoryStructure);
            Assert.Single(apiDefinition.DirectoryStructure.DirectoryNames);
            Assert.Equal("pets", apiDefinition.DirectoryStructure.DirectoryNames.Single());

            IDirectoryStructure subDirectory = apiDefinition.DirectoryStructure.TraverseTo("/pets");

            Assert.Single(subDirectory.RequestInfo.Methods);
            Assert.Contains("post", subDirectory.RequestInfo.Methods, StringComparer.Ordinal);
            Assert.Single(subDirectory.RequestInfo.ContentTypesByMethod["post"]);
            Assert.Contains("application/json", subDirectory.RequestInfo.ContentTypesByMethod["post"]);
        }

        [Fact]
        public void ReadMetadata_WithContentAndMultipleContentTypes_ReturnsApiDefinitionWithContentTypes()
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
          ""required"": false,
          ""content"": {
            ""application/json"": {
            },
            ""text/plain"": {
            }
          }
        }
      }
    }
  }
}";
            JObject jobject = JObject.Parse(json);
            OpenApiV3ApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiV3ApiDefinitionReader();

            ApiDefinition apiDefinition = openApiV3ApiDefinitionReader.ReadDefinition(jobject, null);

            Assert.NotNull(apiDefinition?.DirectoryStructure);
            Assert.Single(apiDefinition.DirectoryStructure.DirectoryNames);
            Assert.Equal("pets", apiDefinition.DirectoryStructure.DirectoryNames.Single());

            IDirectoryStructure subDirectory = apiDefinition.DirectoryStructure.TraverseTo("/pets");

            Assert.Single(subDirectory.RequestInfo.Methods);
            Assert.Contains("post", subDirectory.RequestInfo.Methods, StringComparer.Ordinal);
            Assert.Equal(2, subDirectory.RequestInfo.ContentTypesByMethod["post"].Count);
            Assert.Contains("application/json", subDirectory.RequestInfo.ContentTypesByMethod["post"]);
            Assert.Contains("text/plain", subDirectory.RequestInfo.ContentTypesByMethod["post"]);
        }

        [Fact]
        public void ReadMetadata_WithValidInput_ReturnsApiDefinition()
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
            OpenApiV3ApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiV3ApiDefinitionReader();

            ApiDefinition apiDefinition = openApiV3ApiDefinitionReader.ReadDefinition(jobject, null);

            Assert.NotNull(apiDefinition?.DirectoryStructure);
            Assert.Single(apiDefinition.DirectoryStructure.DirectoryNames);
            Assert.Equal("pets", apiDefinition.DirectoryStructure.DirectoryNames.Single());

            IDirectoryStructure subDirectory = apiDefinition.DirectoryStructure.TraverseTo("/pets");

            Assert.Equal(2, subDirectory.RequestInfo.Methods.Count);
            Assert.Contains("get", subDirectory.RequestInfo.Methods, StringComparer.Ordinal);
            Assert.Contains("post", subDirectory.RequestInfo.Methods, StringComparer.Ordinal);
        }

        [Fact]
        public void ReadMetadata_WithNoRequestBody_ReturnsApiDefinition()
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
            OpenApiV3ApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiV3ApiDefinitionReader();

            ApiDefinition apiDefinition = openApiV3ApiDefinitionReader.ReadDefinition(jobject, null);

            Assert.NotNull(apiDefinition?.DirectoryStructure);
            Assert.Single(apiDefinition.DirectoryStructure.DirectoryNames);
            Assert.Equal("pets", apiDefinition.DirectoryStructure.DirectoryNames.Single());

            IDirectoryStructure subDirectory = apiDefinition.DirectoryStructure.TraverseTo("/pets");

            Assert.Equal(2, subDirectory.RequestInfo.Methods.Count);
            Assert.Contains("get", subDirectory.RequestInfo.Methods, StringComparer.Ordinal);
            Assert.Contains("post", subDirectory.RequestInfo.Methods, StringComparer.Ordinal);
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
            OpenApiV3ApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiV3ApiDefinitionReader();

            bool? result = openApiV3ApiDefinitionReader.CanHandle(jobject);

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
            OpenApiV3ApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiV3ApiDefinitionReader();

            bool? result = openApiV3ApiDefinitionReader.CanHandle(jobject);

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
            OpenApiV3ApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiV3ApiDefinitionReader();

            bool? result = openApiV3ApiDefinitionReader.CanHandle(jobject);

            Assert.False(result);
        }

        [Fact]
        public void ReadDefinition_WithNoServers_BaseAddressesIsEmpty()
        {
            string json = @"{
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""version"": ""v1""
  },
  ""paths"": {
    ""/pets"": {
    }
  }
}";

            JObject jobject = JObject.Parse(json);
            OpenApiV3ApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiV3ApiDefinitionReader();

            ApiDefinition apiDefinition = openApiV3ApiDefinitionReader.ReadDefinition(jobject, null);

            Assert.NotNull(apiDefinition?.BaseAddresses);
            Assert.Empty(apiDefinition.BaseAddresses);
        }

        [Fact]
        public void ReadDefinition_WithOneServer_BaseAddressesHasOneEntry()
        {
            string json = @"{
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""version"": ""v1""
  },
  ""servers"": [
    {
      ""url"": ""https://localhost/"",
      ""description"": ""First Server Address""
    }
  ],
  ""paths"": {
    ""/pets"": {
    }
  }
}";

            JObject jobject = JObject.Parse(json);
            OpenApiV3ApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiV3ApiDefinitionReader();

            ApiDefinition apiDefinition = openApiV3ApiDefinitionReader.ReadDefinition(jobject, null);

            Assert.NotNull(apiDefinition?.BaseAddresses);
            Assert.Single(apiDefinition.BaseAddresses);
            Assert.Equal("https://localhost/", apiDefinition.BaseAddresses[0].Url.ToString());
            Assert.Equal("First Server Address", apiDefinition.BaseAddresses[0].Description);
        }

        [Fact]
        public void ReadDefinition_WithTwoServers_BaseAddressesHasTwoEntries()
        {
            string json = @"{
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""version"": ""v1""
  },
  ""servers"": [
    {
      ""url"": ""https://petstore.swagger.io/"",
      ""description"": ""Production Server Address""
    },
    {
      ""url"": ""https://localhost/"",
      ""description"": ""Local Development Server Address""
    }
  ],
  ""paths"": {
    ""/pets"": {
    }
  }
}";

            JObject jobject = JObject.Parse(json);
            OpenApiV3ApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiV3ApiDefinitionReader();

            ApiDefinition apiDefinition = openApiV3ApiDefinitionReader.ReadDefinition(jobject, null);

            Assert.NotNull(apiDefinition?.BaseAddresses);
            Assert.Equal(2, apiDefinition.BaseAddresses.Count);

            Assert.Equal("https://petstore.swagger.io/", apiDefinition.BaseAddresses[0].Url.ToString());
            Assert.Equal("Production Server Address", apiDefinition.BaseAddresses[0].Description);

            Assert.Equal("https://localhost/", apiDefinition.BaseAddresses[1].Url.ToString());
            Assert.Equal("Local Development Server Address", apiDefinition.BaseAddresses[1].Description);
        }

        [Fact]
        public void ReadDefinition_WithRelativeServer_BaseAddressesCorrectEntry()
        {
            string json = @"{
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""version"": ""v1""
  },
  ""servers"": [
    {
      ""url"": ""/api/v2"",
      ""description"": ""First Server Address""
    }
  ],
  ""paths"": {
    ""/pets"": {
    }
  }
}";

            JObject jobject = JObject.Parse(json);
            OpenApiV3ApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiV3ApiDefinitionReader();

            ApiDefinition apiDefinition = openApiV3ApiDefinitionReader.ReadDefinition(jobject, new Uri("https://localhost/swagger.json"));

            Assert.NotNull(apiDefinition?.BaseAddresses);
            Assert.Single(apiDefinition.BaseAddresses);
            Assert.Equal("https://localhost/api/v2/", apiDefinition.BaseAddresses[0].Url.ToString());
            Assert.Equal("First Server Address", apiDefinition.BaseAddresses[0].Description);
        }

        [Fact]
        public void ReadDefinition_WithRequestBody_SchemaIsIncluded()
        {
            string contentType = "application/json";
            string json = @"{
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""version"": ""v1""
  },
  ""paths"": {
    ""/pets"": {
      ""post"": {
        ""requestBody"": {
          ""content"": {
            """ + contentType + @""": {
              ""schema"": {
                ""type"": ""object"",
                ""properties"": {
                  ""date"": {
                    ""type"": ""string"",
                    ""format"": ""date-time""
                  }
                }
              }
            }
          }
        }
      }
    }
  }
}";
            JObject jobject = JObject.Parse(json);
            OpenApiV3ApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiV3ApiDefinitionReader();

            ApiDefinition apiDefinition = openApiV3ApiDefinitionReader.ReadDefinition(jobject, new Uri("https://localhost/swagger.json"));
            IDirectoryStructure pets = apiDefinition.DirectoryStructure.TraverseTo("pets");
            string requestBody = pets.RequestInfo.GetRequestBodyForContentType(ref contentType, "post");

            Assert.NotNull(requestBody);
        }
    }
}
