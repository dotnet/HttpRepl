using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.IntegrationTests.Mocks;
using Microsoft.HttpRepl.Resources;
using Microsoft.Repl;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;
using Moq;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class SetSwaggerCommandTests : ICommandTestHelper<SetSwaggerCommand>
    {
        public SetSwaggerCommandTests()
          : base(new SetSwaggerCommand())
        {
        }

        [Fact]
        public void GetHelpDetails_WithInvalidParseResultSection_ReturnsNull()
        {
            string result = GetHelpDetails(parseResultSections: "section1");

            Assert.Null(result);
        }

        [Fact]
        public void GetHelpDetails_WithFirstParseResultSectionNotEqualToName_ReturnsNull()
        {
            string result = GetHelpDetails(parseResultSections: "section1 section2 section3");

            Assert.Null(result);
        }

        [Fact]
        public void GetHelpDetails_WithValidInput_ReturnsDescription()
        {
            string parseResultSections = "set swagger https://localhost:44366/swagger/v1/swagger.json";
            string result = GetHelpDetails(parseResultSections);

            Assert.Equal(Strings.SetSwaggerCommand_Description, result);
        }

        [Fact]
        public void GetHelpSummary_ReturnsDescription()
        {
            string result = GetHelpSummary();

            Assert.Equal(Strings.SetSwaggerCommand_Description, result);
        }

        [Fact]
        public void CanHandle_WithNoParseResultSections_ReturnsNull()
        {
            bool? result = CanHandle(parseResultSections: string.Empty);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithFirstSectionNotEqualToName_ReturnsNull()
        {
            bool? result = CanHandle(parseResultSections: "section1 section2 section3");

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithSecondSectionNotEqualToSubCommand_ReturnsNull()
        {
            bool? result = CanHandle(parseResultSections: "set section2 section3");

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithValidInput_ReturnsTrue()
        {
            string parseResultSections = "set swagger https://localhost:44366/swagger/v1/swagger.json";
            bool? result = CanHandle(parseResultSections);

            Assert.True(result.Value);
        }

        [Fact]
        public void Suggest_WithNoParseResultSections_ReturnsName()
        {
            IEnumerable<string> suggestions = GetSuggestions(parseResultSections: string.Empty);
            string expected = "set";

            Assert.Single(suggestions);
            Assert.Equal(expected, suggestions.First());
        }

        [Fact]
        public void Suggest_WithSelectedSectionAtOne_ReturnsSubCommand()
        {
            IEnumerable<string> suggestions = GetSuggestions(parseResultSections: "set swagger");
            string expected = "swagger";

            Assert.Single(suggestions);
            Assert.Equal(expected, suggestions.First());
        }

        [Fact]
        public async Task ExecuteAsync_WithExactlyOneParseResultSection_WritesToConsoleManagerError()
        {
            MockedShellState shellState = new MockedShellState();
            await ExecuteAsyncWithInvalidParseResultSections(parseResultSections: "section1", shellState);

            VerifyErrorMessageWasWrittenToConsoleManagerError(shellState);
        }

        [Fact]
        public async Task ExecuteAsync_WithExactlyTwoParseResultSections_SetsHttpStateSwaggerStructureToNull()
        {
            MockedShellState shellState = new MockedShellState();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("section1 sections2");
            HttpState httpState = new HttpState(null);
            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            await setSwaggerCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Null(httpState.SwaggerStructure);
        }

        [Fact]
        public async Task ExecuteAsync_WithEmptyThirdParseResultSection_WritesToConsoleManagerError()
        {
            MockedShellState shellState = new MockedShellState();
            string parseResultSections = "section1 sections2  ";

            await ExecuteAsyncWithInvalidParseResultSections(parseResultSections, shellState);

            VerifyErrorMessageWasWrittenToConsoleManagerError(shellState);
        }

        [Fact]
        public async Task ExecuteAsync_WhenThirdSectionIsNotAValidUri_WritesToConsoleManagerError()
        {
            MockedShellState shellState = new MockedShellState();
            string parseResultSections = "section1 sections2 section3";

            await ExecuteAsyncWithInvalidParseResultSections(parseResultSections, shellState);

            VerifyErrorMessageWasWrittenToConsoleManagerError(shellState);
        }

        [Fact]
        public async Task ExecuteAsync_WithoutChildDirectories_SetsHttpStateSwaggerStructure()
        {
            string response = @"{
  ""swagger"": ""2.0"",
  ""paths"": {
    ""/api"": {
      ""get"": {
        ""tags"": [ ""Employees"" ],
        ""operationId"": ""GetEmployee"",
        ""consumes"": [],
        ""produces"": [ ""text/plain"", ""application/json"", ""text/json"" ],
        ""parameters"": [],
        ""responses"": {
          ""200"": {
            ""description"": ""Success"",
            ""schema"": {
              ""uniqueItems"": false,
              ""type"": ""array""
            }
          }
        }
      }
    }
  }
}";
            MockedShellState shellState = new MockedShellState();
            IDirectoryStructure directoryStructure = await GetDirectoryStructure(shellState, response).ConfigureAwait(false);
            List<string> directoryNames = directoryStructure.DirectoryNames.ToList();
            string expectedDirectoryName = "api";

            Assert.Single(directoryNames);
            Assert.Equal("api", directoryNames.First());

            IDirectoryStructure childDirectoryStructure = directoryStructure.GetChildDirectory(expectedDirectoryName);

            Assert.Empty(childDirectoryStructure.DirectoryNames);
        }

        [Fact]
        public async Task ExecuteAsync_WithChildDirectories_SetsHttpStateSwaggerStructureWithChildDirectorStuctureInfo()
        {
            string response = @"{
  ""swagger"": ""2.0"",
  ""paths"": {
    ""/api/Employees"": {
      ""get"": {
        ""tags"": [ ""Employees"" ],
        ""operationId"": ""GetEmployee"",
        ""consumes"": [],
        ""produces"": [ ""text/plain"", ""application/json"", ""text/json"" ],
        ""parameters"": [],
        ""responses"": {
          ""200"": {
            ""description"": ""Success"",
            ""schema"": {
              ""uniqueItems"": false,
              ""type"": ""array""
            }
          }
        }
      }
    },
    ""/api/Values"": {
      ""post"": {
        ""tags"": [ ""Values"" ],
        ""operationId"": ""Post"",
        ""consumes"": [ ""application/json-patch+json"", ""application/json"", ""text/json"", ""application/*+json"" ],
        ""produces"": [],
        ""parameters"": [
          {
            ""name"": ""value"",
            ""in"": ""body"",
            ""required"": false,
            ""schema"": { ""type"": ""string"" }
          }
        ],
        ""responses"": { ""200"": { ""description"": ""Success"" } }
      }
    }
  }
}";
            MockedShellState shellState = new MockedShellState();
            IDirectoryStructure directoryStructure = await GetDirectoryStructure(shellState, response).ConfigureAwait(false);
            List<string> directoryNames = directoryStructure.DirectoryNames.ToList();
            string expectedDirectoryName = "api";

            Assert.Single(directoryNames);
            Assert.Equal(expectedDirectoryName, directoryNames.First());

            IDirectoryStructure childDirectoryStructure = directoryStructure.GetChildDirectory(expectedDirectoryName);
            List<string> childDirectoryNames = childDirectoryStructure.DirectoryNames.ToList();

            Assert.Equal(2, childDirectoryNames.Count);
            Assert.Equal("Employees", childDirectoryNames.First());
            Assert.Equal("Values", childDirectoryNames.ElementAt(1));
        }

        private async Task<IDirectoryStructure> GetDirectoryStructure(MockedShellState shellState, string response)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage();
            responseMessage.Content = new MockHttpContent(response);
            MockHttpMessageHandler messageHandler = new MockHttpMessageHandler(responseMessage);
            HttpClient client = new HttpClient(messageHandler);
            HttpState httpState = new HttpState(client);
            ICoreParseResult parseResult = CoreParseResultHelper.Create("section1 section1 http://localhost:5050/somePath");
            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            await setSwaggerCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            return httpState.SwaggerStructure;
        }
    }
}
