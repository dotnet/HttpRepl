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
        public async Task NoBasePath_ReturnsMessage()
        {
            HttpState httpState = new HttpState();

            string expectedErrorMessage = StringResources.Error_NoBasePath.SetColor(httpState.ErrorColor);
            string actualErrorMessage = null;

            IShellState shellState = MockHelpers.GetMockedShellState(errorMessageCallback: (s) => actualErrorMessage = s);

            PostCommand command = new PostCommand();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("POST");
            CancellationTokenSource cts = new CancellationTokenSource();

            await command.ExecuteAsync(shellState, httpState, parseResult, cts.Token);

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
            Port = 5051;
            Routes.Add(new StaticSampleApiServerRoute("POST", "this/is/a/test/route", "This is a test response from a POST."));
        }
    }
}
