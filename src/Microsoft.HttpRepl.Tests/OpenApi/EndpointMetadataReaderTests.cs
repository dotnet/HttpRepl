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
            EndpointMetadataReader endpointMetadataReader = new EndpointMetadataReader();

            string json = @"{
  ""info"": {
    ""version"": ""v1"",
    ""title"": ""My API""
  }
}";

            JObject jobject = JObject.Parse(json);

            Assert.Null(endpointMetadataReader.Read(jobject));
        }

        [Fact]
        public void Read_WithSwagger2KeyInInput_ReturnsEndpointMetadata()
        {
            EndpointMetadataReader endpointMetadataReader = new EndpointMetadataReader();

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
      ""put"": {
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

            JObject jobject = JObject.Parse(json);
            List<EndpointMetadata> endpointMetadata = endpointMetadataReader.Read(jobject).ToList();
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<Parameter>>> availableRequests = endpointMetadata[0].AvailableRequests;
            KeyValuePair<string, IReadOnlyDictionary<string, IReadOnlyList<Parameter>>>  firstRequest = availableRequests.First();
            KeyValuePair<string, IReadOnlyDictionary<string, IReadOnlyList<Parameter>>> secondRequest = availableRequests.ElementAt(1);

            Assert.Single(endpointMetadata);
            Assert.Equal("/api/Employees", endpointMetadata[0].Path);

            Assert.Equal(2, availableRequests.Count);
            Assert.Equal("get", firstRequest.Key);
            Assert.Equal("put", secondRequest.Key);
        }
    }
}
