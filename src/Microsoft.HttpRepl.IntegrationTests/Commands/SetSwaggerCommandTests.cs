using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.IntegrationTests.Mocks;
using Microsoft.Repl;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;
using Moq;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class SetSwaggerCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_WithExactlyOneParseResultSection_WritesToConsoleManagerError()
        {
            MockedShellState shellState = new MockedShellState();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("section1");
            HttpState httpState = new HttpState(null);

            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            await setSwaggerCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

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
            ICoreParseResult parseResult = CoreParseResultHelper.Create("section1 section1  ");
            HttpState httpState = new HttpState(null);
            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            await setSwaggerCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            VerifyErrorMessageWasWrittenToConsoleManagerError(shellState);
        }

        [Fact]
        public async Task ExecuteAsync_WhenThirdSectionIsNotAValidUri_WritesToConsoleManagerError()
        {
            MockedShellState shellState = new MockedShellState();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("section1 section1 section3");
            HttpState httpState = new HttpState(null);
            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            await setSwaggerCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

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

        [Fact]
        public void GetHelpDetails_WithInvalidParseResultSection_ReturnsNull()
        {
            ICoreParseResult parseResult = CoreParseResultHelper.Create("section1");
            MockedShellState shellState = new MockedShellState();
            HttpState httpState = new HttpState(null);
            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            string result = setSwaggerCommand.GetHelpDetails(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void GetHelpDetails_WithFirstParseResultSectionNotEqualToName_ReturnsNull()
        {
            ICoreParseResult parseResult = CoreParseResultHelper.Create("section1 section2 section3");
            MockedShellState shellState = new MockedShellState();
            HttpState httpState = new HttpState(null);
            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            string result = setSwaggerCommand.GetHelpDetails(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void GetHelpDetails_WithValidInput_ReturnsDescription()
        {
            ICoreParseResult parseResult = CoreParseResultHelper.Create("set swagger https://localhost:44366/swagger/v1/swagger.json");
            MockedShellState shellState = new MockedShellState();
            HttpState httpState = new HttpState(null);
            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            string result = setSwaggerCommand.GetHelpDetails(shellState, httpState, parseResult);

            Assert.Equal(setSwaggerCommand.Description, result);
        }

        [Fact]
        public void GetHelpSummary_ReturnsDescription()
        {
            MockedShellState shellState = new MockedShellState();
            HttpState httpState = new HttpState(null);
            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            string result = setSwaggerCommand.GetHelpSummary(shellState, httpState);

            Assert.Equal(setSwaggerCommand.Description, result);
        }

        [Fact]
        public void CanHandle_WithNoParseResultSections_ReturnsNull()
        {
            ICoreParseResult parseResult = CoreParseResultHelper.Create(string.Empty);
            MockedShellState shellState = new MockedShellState();
            HttpState httpState = new HttpState(null);
            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            bool? result = setSwaggerCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithFirstSectionNotEqualToName_ReturnsNull()
        {
            ICoreParseResult parseResult = CoreParseResultHelper.Create("section1 section2 section3");
            MockedShellState shellState = new MockedShellState();
            HttpState httpState = new HttpState(null);
            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            bool? result = setSwaggerCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithSecondSectionNotEqualToSubCommand_ReturnsNull()
        {
            ICoreParseResult parseResult = CoreParseResultHelper.Create("set section2 section3");
            MockedShellState shellState = new MockedShellState();
            HttpState httpState = new HttpState(null);
            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            bool? result = setSwaggerCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithValidInput_ReturnsTrue()
        {
            ICoreParseResult parseResult = CoreParseResultHelper.Create("set swagger https://localhost:44366/swagger/v1/swagger.json");
            MockedShellState shellState = new MockedShellState();
            HttpState httpState = new HttpState(null);
            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            bool? result = setSwaggerCommand.CanHandle(shellState, httpState, parseResult);

            Assert.True(result.Value);
        }

        [Fact]
        public void Suggest_WithNoParseResultSections_ReturnsName()
        {
            ICoreParseResult parseResult = CoreParseResultHelper.Create(string.Empty);
            MockedShellState shellState = new MockedShellState();
            HttpState httpState = new HttpState(null);
            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            IEnumerable<string> suggestions = setSwaggerCommand.Suggest(shellState, httpState, parseResult);
            string expected = "set";

            Assert.Single(suggestions);
            Assert.Equal(expected, suggestions.First());
        }

        [Fact]
        public void Suggest_WithSelectedSectionAtOne_ReturnsSubCommand()
        {
            ICoreParseResult parseResult = CoreParseResultHelper.Create("set swagger");
            MockedShellState shellState = new MockedShellState();
            HttpState httpState = new HttpState(null);
            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            IEnumerable<string> suggestions = setSwaggerCommand.Suggest(shellState, httpState, parseResult);
            string expected = "swagger";

            Assert.Single(suggestions);
            Assert.Equal(expected, suggestions.First());
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

        private void VerifyErrorMessageWasWrittenToConsoleManagerError(IShellState shellState)
        {
            Mock<IWritable> error = Mock.Get(shellState.ConsoleManager.Error);

            error.Verify(s => s.WriteLine(It.IsAny<string>()), Times.Once);
        }
    }
}
