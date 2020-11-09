// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.HttpRepl.OpenApi;
using Xunit;

namespace Microsoft.HttpRepl.Tests.OpenApi
{
    public class OpenApiDotNetApiDefinitionReaderV3Tests
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

            OpenApiDotNetApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            ApiDefinitionParseResult result = openApiV3ApiDefinitionReader.ReadDefinition(json, null);

            Assert.NotNull(result.ApiDefinition?.DirectoryStructure);
            Assert.Empty(result.ApiDefinition.DirectoryStructure.DirectoryNames);
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

            OpenApiDotNetApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            ApiDefinitionParseResult result = openApiV3ApiDefinitionReader.ReadDefinition(json, null);

            Assert.NotNull(result.ApiDefinition?.DirectoryStructure);
            Assert.Empty(result.ApiDefinition.DirectoryStructure.DirectoryNames);
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

            OpenApiDotNetApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            ApiDefinitionParseResult result = openApiV3ApiDefinitionReader.ReadDefinition(json, null);

            Assert.NotNull(result.ApiDefinition?.DirectoryStructure);
            Assert.Single(result.ApiDefinition.DirectoryStructure.DirectoryNames);
            Assert.Equal("pets", result.ApiDefinition.DirectoryStructure.DirectoryNames.Single());

            IDirectoryStructure subDirectory = result.ApiDefinition.DirectoryStructure.TraverseTo("/pets");

            Assert.Single(subDirectory.RequestInfo.Methods);
            Assert.Contains("Post", subDirectory.RequestInfo.Methods, StringComparer.Ordinal);
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

            OpenApiDotNetApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            ApiDefinitionParseResult result = openApiV3ApiDefinitionReader.ReadDefinition(json, null);

            Assert.NotNull(result.ApiDefinition?.DirectoryStructure);
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

            OpenApiDotNetApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            ApiDefinitionParseResult result = openApiV3ApiDefinitionReader.ReadDefinition(json, null);

            IDirectoryStructure subDirectory = result.ApiDefinition.DirectoryStructure.TraverseTo("/pets");

            Assert.Null(subDirectory.RequestInfo);
        }

        [Theory]
        [InlineData("Get", true)]
        [InlineData("Post", true)]
        [InlineData("Put", true)]
        [InlineData("Delete", true)]
        [InlineData("Options", true)]
        [InlineData("Head", true)]
        [InlineData("Patch", true)]
        [InlineData("Trace", true)]
        [InlineData("$ref", false)]
        [InlineData("summary", false)]
        [InlineData("description", false)]
        [InlineData("servers", false)]
        [InlineData("parameters", false)]
        [InlineData("", false)]
        public void ReadMetadata_WithSpecifiedMethodName_ReturnsApiDefinitionWithCorrectNumberOfRequestMethods(string method, bool shouldHaveRequest)
        {
            // The method must be lowercase to be valid in the json
            string methodForJson = method.ToLower();
            string json = @"{
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""title"": ""OpenAPI v3 Spec"",
    ""version"": ""v1""
  },
  ""paths"": {
    ""/pets"": {
      """ + methodForJson + @""": {
        ""summary"": ""Do something"",
        ""operationId"": ""doSomething"",
        ""responses"": {
          ""200"": {
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

            OpenApiDotNetApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            ApiDefinitionParseResult result = openApiV3ApiDefinitionReader.ReadDefinition(json, null);

            Assert.NotNull(result.ApiDefinition?.DirectoryStructure);
            Assert.Single(result.ApiDefinition.DirectoryStructure.DirectoryNames);
            Assert.Equal("pets", result.ApiDefinition.DirectoryStructure.DirectoryNames.Single());

            IDirectoryStructure subDirectory = result.ApiDefinition.DirectoryStructure.TraverseTo("/pets");

            if (shouldHaveRequest)
            {
                Assert.Single(subDirectory.RequestInfo.Methods);
                Assert.Contains(method, subDirectory.RequestInfo.Methods, StringComparer.OrdinalIgnoreCase);
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
  ""info"": {
    ""title"": ""OpenAPI v3 Spec"",
    ""version"": ""v1""
  },
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

            OpenApiDotNetApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            ApiDefinitionParseResult result = openApiV3ApiDefinitionReader.ReadDefinition(json, null);

            Assert.NotNull(result.ApiDefinition?.DirectoryStructure);
            Assert.Single(result.ApiDefinition.DirectoryStructure.DirectoryNames);
            Assert.Equal("pets", result.ApiDefinition.DirectoryStructure.DirectoryNames.Single());

            IDirectoryStructure subDirectory = result.ApiDefinition.DirectoryStructure.TraverseTo("/pets");

            Assert.Single(subDirectory.RequestInfo.Methods);
            Assert.Contains("Post", subDirectory.RequestInfo.Methods, StringComparer.Ordinal);
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

            OpenApiDotNetApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            ApiDefinitionParseResult result = openApiV3ApiDefinitionReader.ReadDefinition(json, null);

            Assert.NotNull(result.ApiDefinition?.DirectoryStructure);
            Assert.Single(result.ApiDefinition.DirectoryStructure.DirectoryNames);
            Assert.Equal("pets", result.ApiDefinition.DirectoryStructure.DirectoryNames.Single());

            IDirectoryStructure subDirectory = result.ApiDefinition.DirectoryStructure.TraverseTo("/pets");

            Assert.Single(subDirectory.RequestInfo.Methods);
            Assert.Contains("Post", subDirectory.RequestInfo.Methods, StringComparer.Ordinal);
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

            OpenApiDotNetApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            ApiDefinitionParseResult result = openApiV3ApiDefinitionReader.ReadDefinition(json, null);

            Assert.NotNull(result.ApiDefinition?.DirectoryStructure);
            Assert.Single(result.ApiDefinition.DirectoryStructure.DirectoryNames);
            Assert.Equal("pets", result.ApiDefinition.DirectoryStructure.DirectoryNames.Single());

            IDirectoryStructure subDirectory = result.ApiDefinition.DirectoryStructure.TraverseTo("/pets");

            Assert.Single(subDirectory.RequestInfo.Methods);
            Assert.Contains("Post", subDirectory.RequestInfo.Methods, StringComparer.Ordinal);
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

            OpenApiDotNetApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            ApiDefinitionParseResult result = openApiV3ApiDefinitionReader.ReadDefinition(json, null);

            Assert.NotNull(result.ApiDefinition?.DirectoryStructure);
            Assert.Single(result.ApiDefinition.DirectoryStructure.DirectoryNames);
            Assert.Equal("pets", result.ApiDefinition.DirectoryStructure.DirectoryNames.Single());

            IDirectoryStructure subDirectory = result.ApiDefinition.DirectoryStructure.TraverseTo("/pets");

            Assert.Equal(2, subDirectory.RequestInfo.Methods.Count);
            Assert.Contains("Get", subDirectory.RequestInfo.Methods, StringComparer.Ordinal);
            Assert.Contains("Post", subDirectory.RequestInfo.Methods, StringComparer.Ordinal);
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

            OpenApiDotNetApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            ApiDefinitionParseResult result = openApiV3ApiDefinitionReader.ReadDefinition(json, null);

            Assert.NotNull(result.ApiDefinition?.DirectoryStructure);
            Assert.Single(result.ApiDefinition.DirectoryStructure.DirectoryNames);
            Assert.Equal("pets", result.ApiDefinition.DirectoryStructure.DirectoryNames.Single());

            IDirectoryStructure subDirectory = result.ApiDefinition.DirectoryStructure.TraverseTo("/pets");

            Assert.Equal(2, subDirectory.RequestInfo.Methods.Count);
            Assert.Contains("Get", subDirectory.RequestInfo.Methods, StringComparer.Ordinal);
            Assert.Contains("Post", subDirectory.RequestInfo.Methods, StringComparer.Ordinal);
        }

        [Fact]
        public void CanHandle_WithNoOpenApiKeyInDocument_ReturnsFalse()
        {
            string json = @"{
  ""info"": {
    ""title"": ""OpenAPI v? Spec"",
    ""version"": ""v1""
  },
   ""paths"": {
  }
}";

            OpenApiDotNetApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            ApiDefinitionParseResult result = openApiV3ApiDefinitionReader.CanHandle(json);

            Assert.False(result.Success);
        }

        [Fact]
        public void CanHandle_WithValidOpenApiVersionInDocument_ReturnsTrue()
        {
            string json = @"{
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""title"": ""OpenAPI v3 Spec"",
    ""version"": ""v1""
  },
   ""paths"": {
  }
}";

            OpenApiDotNetApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            ApiDefinitionParseResult result = openApiV3ApiDefinitionReader.CanHandle(json);

            Assert.True(result.Success);
        }

        [Fact]
        public void CanHandle_WithOpenApiVersionGreaterThanThree_ReturnsFalse()
        {
            string json = @"{
  ""openapi"": ""4.0.0"",
  ""info"": {
    ""title"": ""OpenAPI v4 Spec"",
    ""version"": ""v1""
  },
   ""paths"": {
  }
}";

            OpenApiDotNetApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            ApiDefinitionParseResult result = openApiV3ApiDefinitionReader.CanHandle(json);

            Assert.False(result.Success);
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


            OpenApiDotNetApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            ApiDefinitionParseResult result = openApiV3ApiDefinitionReader.ReadDefinition(json, null);

            Assert.NotNull(result.ApiDefinition?.BaseAddresses);
            Assert.Empty(result.ApiDefinition.BaseAddresses);
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


            OpenApiDotNetApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            ApiDefinitionParseResult result = openApiV3ApiDefinitionReader.ReadDefinition(json, null);

            Assert.NotNull(result.ApiDefinition?.BaseAddresses);
            Assert.Single(result.ApiDefinition.BaseAddresses);
            Assert.Equal("https://localhost/", result.ApiDefinition.BaseAddresses[0].Url.ToString());
            Assert.Equal("First Server Address", result.ApiDefinition.BaseAddresses[0].Description);
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


            OpenApiDotNetApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            ApiDefinitionParseResult result = openApiV3ApiDefinitionReader.ReadDefinition(json, null);

            Assert.NotNull(result.ApiDefinition?.BaseAddresses);
            Assert.Equal(2, result.ApiDefinition.BaseAddresses.Count);

            Assert.Equal("https://petstore.swagger.io/", result.ApiDefinition.BaseAddresses[0].Url.ToString());
            Assert.Equal("Production Server Address", result.ApiDefinition.BaseAddresses[0].Description);

            Assert.Equal("https://localhost/", result.ApiDefinition.BaseAddresses[1].Url.ToString());
            Assert.Equal("Local Development Server Address", result.ApiDefinition.BaseAddresses[1].Description);
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


            OpenApiDotNetApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            ApiDefinitionParseResult result = openApiV3ApiDefinitionReader.ReadDefinition(json, new Uri("https://localhost/swagger.json"));

            Assert.NotNull(result.ApiDefinition?.BaseAddresses);
            Assert.Single(result.ApiDefinition.BaseAddresses);
            Assert.Equal("https://localhost/api/v2/", result.ApiDefinition.BaseAddresses[0].Url.ToString());
            Assert.Equal("First Server Address", result.ApiDefinition.BaseAddresses[0].Description);
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

            OpenApiDotNetApiDefinitionReader openApiV3ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            ApiDefinitionParseResult result = openApiV3ApiDefinitionReader.ReadDefinition(json, new Uri("https://localhost/swagger.json"));
            IDirectoryStructure pets = result.ApiDefinition.DirectoryStructure.TraverseTo("pets");
            string requestBody = pets.RequestInfo.GetRequestBodyForContentType(ref contentType, "post");

            Assert.NotNull(requestBody);
        }
    }
}
