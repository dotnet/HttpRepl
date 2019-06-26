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
        public async Task ExecuteAsync_WithExactlyTwoParseResult_ReturnsEmptyTask()
        {
            MockedShellState shellState = new MockedShellState();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("section1 sections2");
            string baseAddress = "http://localhost:5050"; 
            HttpState httpState = HttpStateHelpers.Create(baseAddress);

            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            await setSwaggerCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Null(httpState.SwaggerStructure);
        }

        [Fact]
        public async Task ExecuteAsync_WithExactlyOneParseResult_WritesToConsoleManagerError()
        {
            MockedShellState shellState = new MockedShellState();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("section1");
            string baseAddress = "http://localhost:5050";
            HttpState httpState = HttpStateHelpers.Create(baseAddress);

            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            await setSwaggerCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            VerifyErrorMessageWasWrittenToConsoleManagerError(shellState);
        }

        [Fact]
        public async Task ExecuteAsync_WithEmptyThirdParseResultSection_WritesToConsoleManagerError()
        {
            MockedShellState shellState = new MockedShellState();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("section1 section1  ");
            string baseAddress = "http://localhost:5050";
            HttpState httpState = HttpStateHelpers.Create(baseAddress);

            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            await setSwaggerCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            VerifyErrorMessageWasWrittenToConsoleManagerError(shellState);
        }

        [Fact]
        public async Task ExecuteAsync_WhenThirdSectionIsNotAValidUri_WritesToConsoleManagerError()
        {
            MockedShellState shellState = new MockedShellState();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("section1 section1 section3");
            string baseAddress = "http://localhost:5050";
            HttpState httpState = HttpStateHelpers.Create(baseAddress);

            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            await setSwaggerCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            VerifyErrorMessageWasWrittenToConsoleManagerError(shellState);
        }

        [Fact]
        public async Task ExecuteAsync_WithValidInput_SetsHttpStateSwaggerStructure()
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
    }
  }
}";

            MockedShellState shellState = new MockedShellState();
            IDirectoryStructure directoryStructure = await GetDirectoryStructure(shellState, response).ConfigureAwait(false);
            List<string> directoryNames = directoryStructure.DirectoryNames.ToList();

            Assert.Single(directoryNames);
            Assert.Equal("api", directoryNames.First());
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
