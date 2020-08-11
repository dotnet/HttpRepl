// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.HttpRepl.OpenApi;
using Xunit;

namespace Microsoft.HttpRepl.Tests.OpenApi
{
    public class OpenApiDotNetApiDefinitionReaderV2Tests
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

            OpenApiDotNetApiDefinitionReader swaggerV2ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            ApiDefinition apiDefinition = swaggerV2ApiDefinitionReader.ReadDefinition(json, null);

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

            OpenApiDotNetApiDefinitionReader swaggerV2ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            ApiDefinition apiDefinition = swaggerV2ApiDefinitionReader.ReadDefinition(json, null);

            Assert.NotNull(apiDefinition?.DirectoryStructure);
            Assert.Empty(apiDefinition.DirectoryStructure.DirectoryNames);
        }

        [Fact]
        public void ReadMetadata_WithNoRequestMethods_ReturnsApiDefinitionWithNullRequestInfo()
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

            OpenApiDotNetApiDefinitionReader swaggerV2ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            ApiDefinition apiDefinition = swaggerV2ApiDefinitionReader.ReadDefinition(json, null);

            IDirectoryStructure subDirectory = apiDefinition.DirectoryStructure.TraverseTo("/api/Employees");

            Assert.Null(subDirectory.RequestInfo);
        }

        [Fact]
        public void ReadMetadata_WithNoRequestMethods_ReturnsApiDefinitionWithStructure()
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

            OpenApiDotNetApiDefinitionReader swaggerV2ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            ApiDefinition apiDefinition = swaggerV2ApiDefinitionReader.ReadDefinition(json, null);

            Assert.NotNull(apiDefinition?.DirectoryStructure);
        }

        [Fact]
        public void ReadMetadata_WithValidInput_ReturnsApiDefinition()
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

            OpenApiDotNetApiDefinitionReader swaggerV2ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            ApiDefinition apiDefinition = swaggerV2ApiDefinitionReader.ReadDefinition(json, null);

            Assert.NotNull(apiDefinition?.DirectoryStructure);
            Assert.Single(apiDefinition.DirectoryStructure.DirectoryNames);
            Assert.Equal("api", apiDefinition.DirectoryStructure.DirectoryNames.Single());

            IDirectoryStructure subDirectory = apiDefinition.DirectoryStructure.TraverseTo("/api/Employees");

            Assert.Equal(2, subDirectory.RequestInfo.Methods.Count);
            Assert.Contains("Get", subDirectory.RequestInfo.Methods, StringComparer.Ordinal);
            Assert.Contains("Post", subDirectory.RequestInfo.Methods, StringComparer.Ordinal);
        }

        [Fact]
        public void CanHandle_WithNoSwaggerVersionKeyInDocument_ReturnsFalse()
        {
            string json = @"{
  ""info"": {
    ""title"": ""OpenAPI v? Spec"",
    ""version"": ""v1""
  },
  ""paths"": {
  }
}";

            OpenApiDotNetApiDefinitionReader swaggerV2ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            bool? result = swaggerV2ApiDefinitionReader.CanHandle(json);

            Assert.False(result);
        }

        [Fact]
        public void CanHandle_WithValidSwaggerVersionKeyInDocument_ReturnsTrue()
        {
            string json = @"{
  ""swagger"": ""2.0"",
  ""info"": {
    ""title"": ""OpenAPI v2 Spec"",
    ""version"": ""v1""
  },
  ""paths"": {
  }
}";

            OpenApiDotNetApiDefinitionReader swaggerV2ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            bool? result = swaggerV2ApiDefinitionReader.CanHandle(json);

            Assert.True(result);
        }

        [Fact]
        public void ReadDefinition_WithNoHost_BaseAddressesIsEmpty()
        {
            string json = @"{
  ""swagger"": ""2.0"",
  ""info"": {
    ""version"": ""v1""
  },
  ""paths"": {
  }
}";

            OpenApiDotNetApiDefinitionReader swaggerV2ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            ApiDefinition apiDefinition = swaggerV2ApiDefinitionReader.ReadDefinition(json, new Uri("http://localhost/swagger.json"));

            Assert.NotNull(apiDefinition?.BaseAddresses);
            Assert.Empty(apiDefinition.BaseAddresses);
        }

        [Fact]
        public void ReadDefinition_WithHostAndOneScheme_BaseAddressesHasOneEntry()
        {
            string json = @"{
  ""swagger"": ""2.0"",
  ""info"": {
    ""version"": ""v1""
  },
  ""host"": ""localhost"",
  ""schemes"": [
    ""https""
  ],
  ""paths"": {
  }
}";

            OpenApiDotNetApiDefinitionReader swaggerV2ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            ApiDefinition apiDefinition = swaggerV2ApiDefinitionReader.ReadDefinition(json, new Uri("http://localhost/swagger.json"));

            Assert.NotNull(apiDefinition?.BaseAddresses);
            Assert.Single(apiDefinition.BaseAddresses);
            Assert.Equal("https://localhost/", apiDefinition.BaseAddresses[0].Url.ToString(), StringComparer.Ordinal);
        }

        [Fact]
        public void ReadDefinition_WithHostAndTwoSchemes_BaseAddressesHasTwoEntries()
        {
            string json = @"{
  ""swagger"": ""2.0"",
  ""info"": {
    ""version"": ""v1""
  },
  ""host"": ""localhost"",
  ""schemes"": [
    ""https"",
    ""http""
  ],
  ""paths"": {
  }
}";

            OpenApiDotNetApiDefinitionReader swaggerV2ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            ApiDefinition apiDefinition = swaggerV2ApiDefinitionReader.ReadDefinition(json, new Uri("http://localhost/swagger.json"));

            Assert.NotNull(apiDefinition?.BaseAddresses);
            Assert.Equal(2, apiDefinition.BaseAddresses.Count);
            Assert.Equal("https://localhost/", apiDefinition.BaseAddresses[0].Url.ToString(), StringComparer.Ordinal);
            Assert.Equal("http://localhost/", apiDefinition.BaseAddresses[1].Url.ToString(), StringComparer.Ordinal);
        }

        [Fact]
        public void ReadDefinition_WithHostAndNoScheme_BaseAddressesHasOneEntryWithSchemeFromSourceUri()
        {
            string json = @"{
  ""swagger"": ""2.0"",
  ""info"": {
    ""version"": ""v1""
  },
  ""host"": ""localhost"",
  ""paths"": {
  }
}";

            OpenApiDotNetApiDefinitionReader swaggerV2ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            ApiDefinition apiDefinition = swaggerV2ApiDefinitionReader.ReadDefinition(json, new Uri("https://localhost/swagger.json"));

            Assert.NotNull(apiDefinition?.BaseAddresses);
            Assert.Single(apiDefinition.BaseAddresses);
            Assert.Equal("https://localhost/", apiDefinition.BaseAddresses[0].Url.ToString(), StringComparer.Ordinal);

            apiDefinition = swaggerV2ApiDefinitionReader.ReadDefinition(json, new Uri("http://localhost/swagger.json"));

            Assert.NotNull(apiDefinition?.BaseAddresses);
            Assert.Single(apiDefinition.BaseAddresses);
            Assert.Equal("http://localhost/", apiDefinition.BaseAddresses[0].Url.ToString(), StringComparer.Ordinal);
        }

        [Fact]
        public void ReadDefinition_WithHostAndBaseAndScheme_BaseAddressesHasOneEntry()
        {
            string json = @"{
  ""swagger"": ""2.0"",
  ""info"": {
    ""version"": ""v1""
  },
  ""host"": ""localhost"",
  ""basePath"": ""/api/v2"",
  ""schemes"": [
    ""https""
  ],
  ""paths"": {
  }
}";

            OpenApiDotNetApiDefinitionReader swaggerV2ApiDefinitionReader = new OpenApiDotNetApiDefinitionReader();

            ApiDefinition apiDefinition = swaggerV2ApiDefinitionReader.ReadDefinition(json, new Uri("http://localhost/swagger.json"));

            Assert.NotNull(apiDefinition?.BaseAddresses);
            Assert.Single(apiDefinition.BaseAddresses);
            Assert.Equal("https://localhost/api/v2/", apiDefinition.BaseAddresses[0].Url.ToString(), StringComparer.Ordinal);
        }
    }
}
