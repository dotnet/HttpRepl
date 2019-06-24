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
    public class OptionsCommandTests : IClassFixture<OptionsCommandsFixture>
    {
        private readonly OptionsCommandsConfig _config;
        public OptionsCommandTests(OptionsCommandsFixture OptionsCommandsFixture)
        {
            _config = OptionsCommandsFixture.Config;
        }

        [Fact]
        public async Task ExecuteAsync_WithNoBasePath_VerifyError()
        {
            HttpState httpState = new HttpState();

            string expectedErrorMessage = Resources.Strings.Error_NoBasePath.SetColor(httpState.ErrorColor);
            string actualErrorMessage = null;

            IShellState shellState = MockHelpers.GetMockedShellState(errorMessageCallback: (s) => actualErrorMessage = s);

            OptionsCommand command = new OptionsCommand();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("OPTIONS");

            await command.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(expectedErrorMessage, actualErrorMessage);
        }

        [Fact]
        public async Task ExecuteAsync_WithMultipartRoute_VerifyOutput()
        {
            int expectedResponseLines = 6; // The path, four headers and a blank response.
            string expectedResponseContent = "X-HTTPREPL-TESTHEADER: Header value for OPTIONS request with route.";
            List<string> actual = new List<string>();

            IShellState shellState = MockHelpers.GetMockedShellState(writeLineCallback: (s) => actual.Add(s));

            HttpState httpState = HttpStateHelpers.Create(_config.BaseAddress, "this/is/a/test/route");

            OptionsCommand command = new OptionsCommand();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("OPTIONS");

            await command.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(expectedResponseLines, actual.Count);
            Assert.Equal(expectedResponseContent, actual[expectedResponseLines - 2]);
        }

        [Fact]
        public async Task ExecuteAsync_WithOnlyBaseAddress_VerifyOutput()
        {
            int expectedResponseLines = 6; // The path, four headers and a blank response.
            string expectedResponseContent = "X-HTTPREPL-TESTHEADER: Header value for root OPTIONS request.";
            List<string> actual = new List<string>();

            IShellState shellState = MockHelpers.GetMockedShellState(writeLineCallback: (s) => actual.Add(s));

            HttpState httpState = HttpStateHelpers.Create(_config.BaseAddress);

            OptionsCommand command = new OptionsCommand();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("OPTIONS");

            await command.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(expectedResponseLines, actual.Count);
            Assert.Equal(expectedResponseContent, actual[expectedResponseLines - 2]);
        }
    }

    public class OptionsCommandsFixture : IDisposable
    {
        private readonly SampleApiServer _testWebServer;
        public OptionsCommandsConfig Config { get; } = new OptionsCommandsConfig();

        public OptionsCommandsFixture()
        {
            _testWebServer = new SampleApiServer(Config);
            _testWebServer.Start();
        }

        public void Dispose()
        {
            _testWebServer.Stop();
        }
    }

    public class OptionsCommandsConfig : SampleApiServerConfig
    {
        public OptionsCommandsConfig()
        {
            Port = 5053;
            Routes.Add(new DynamicSampleApiServerRoute("OPTIONS", "", context =>
            {
                context.Response.Headers.Add("X-HTTPREPL-TESTHEADER", "Header value for root OPTIONS request.");
                return Task.CompletedTask;
            }));
            Routes.Add(new DynamicSampleApiServerRoute("OPTIONS", "this/is/a/test/route", context =>
            {
                context.Response.Headers.Add("X-HTTPREPL-TESTHEADER", "Header value for OPTIONS request with route.");
                return Task.CompletedTask;
            }));
        }
    }
}
