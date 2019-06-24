using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.Tests.SampleApi;
using Microsoft.Repl;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class DeleteCommandTests : IClassFixture<DeleteCommandsFixture>
    {
        private readonly DeleteCommandsConfig _config;
        public DeleteCommandTests(DeleteCommandsFixture DeleteCommandsFixture)
        {
            _config = DeleteCommandsFixture.Config;
        }

        [Fact]
        public async Task ExecuteAsync_WithNoBasePath_VerifyError()
        {
            HttpState httpState = new HttpState();

            string expectedErrorMessage = Resources.Strings.Error_NoBasePath.SetColor(httpState.ErrorColor);
            string actualErrorMessage = null;

            IShellState shellState = MockHelpers.GetMockedShellState(errorMessageCallback: (s) => actualErrorMessage = s);

            DeleteCommand command = new DeleteCommand();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("DELETE");

            await command.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(expectedErrorMessage, actualErrorMessage);
        }

        [Fact]
        public async Task ExecuteAsync_WithMultipartRoute_VerifyOutput()
        {
            int expectedResponseLines = 5; // The path, three headers and the response.
            string expectedResponseContent = "File path delete received successfully.";
            List<string> actual = new List<string>();

            IShellState shellState = MockHelpers.GetMockedShellState(writeLineCallback: (s) => actual.Add(s));

            HttpState httpState = HttpStateHelpers.Create(_config.BaseAddress, "a/file/path.txt");

            DeleteCommand command = new DeleteCommand();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("DELETE");

            await command.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(expectedResponseLines, actual.Count);
            Assert.Equal(expectedResponseContent, actual[expectedResponseLines - 1]);
        }

        [Fact]
        public async Task ExecuteAsync_WithOnlyBaseAddress_VerifyOutput()
        {
            int expectedResponseLines = 5; // The path, three headers and the response.
            string expectedResponseContent = "Root delete received successfully.";
            List<string> actual = new List<string>();

            IShellState shellState = MockHelpers.GetMockedShellState(writeLineCallback: (s) => actual.Add(s));

            HttpState httpState = HttpStateHelpers.Create(_config.BaseAddress);

            DeleteCommand command = new DeleteCommand();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("DELETE");

            await command.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(expectedResponseLines, actual.Count);
            Assert.Equal(expectedResponseContent, actual[expectedResponseLines - 1]);
        }
    }

    public class DeleteCommandsFixture : IDisposable
    {
        private readonly SampleApiServer _testWebServer;
        public DeleteCommandsConfig Config { get; } = new DeleteCommandsConfig();

        public DeleteCommandsFixture()
        {
            _testWebServer = new SampleApiServer(Config);
            _testWebServer.Start();
        }

        public void Dispose()
        {
            _testWebServer.Stop();
        }
    }

    public class DeleteCommandsConfig : SampleApiServerConfig
    {
        public DeleteCommandsConfig()
        {
            Port = SampleApiServerPorts.DeleteCommandTests;
            Routes.Add(new StaticSampleApiServerRoute("DELETE", "", "Root delete received successfully."));
            Routes.Add(new StaticSampleApiServerRoute("DELETE", "a/file/path.txt", "File path delete received successfully."));
        }
    }
}
