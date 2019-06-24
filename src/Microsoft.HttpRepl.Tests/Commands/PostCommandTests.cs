using System;
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
    public class PostCommandTests : IClassFixture<PostCommandsFixture>
    {
        private readonly PostCommandsConfig _config;
        public PostCommandTests(PostCommandsFixture postCommandsFixture)
        {
            _config = postCommandsFixture.Config;
        }

        [Fact]
        public async Task ExecuteAsync_WithNoBasePath_VerifyError()
        {
            HttpState httpState = new HttpState();

            string expectedErrorMessage = Resources.Strings.Error_NoBasePath.SetColor(httpState.ErrorColor);
            string actualErrorMessage = null;

            IShellState shellState = MockHelpers.GetMockedShellState(errorMessageCallback: (s) => actualErrorMessage = s);

            PostCommand command = new PostCommand();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("POST");

            await command.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(expectedErrorMessage, actualErrorMessage);
        }
    }

    public class PostCommandsFixture : IDisposable
    {
        private readonly SampleApiServer _testWebServer;
        public PostCommandsConfig Config { get; } = new PostCommandsConfig();

        public PostCommandsFixture()
        {
            _testWebServer = new SampleApiServer(Config);
            _testWebServer.Start();
        }

        public void Dispose()
        {
            _testWebServer.Stop();
        }
    }

    public class PostCommandsConfig : SampleApiServerConfig
    {
        public PostCommandsConfig()
        {
            Port = SampleApiServerPorts.PostCommandTests;
            Routes.Add(new StaticSampleApiServerRoute("POST", "this/is/a/test/route", "This is a test response from a POST."));
        }
    }
}
