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
    public class HeadCommandTests : IClassFixture<HeadCommandsFixture>
    {
        private readonly HeadCommandsConfig _config;
        public HeadCommandTests(HeadCommandsFixture HeadCommandsFixture)
        {
            _config = HeadCommandsFixture.Config;
        }

        [Fact]
        public async Task ExecuteAsync_WithNoBasePath_VerifyError()
        {
            HttpState httpState = new HttpState();

            string expectedErrorMessage = Resources.Strings.Error_NoBasePath.SetColor(httpState.ErrorColor);
            string actualErrorMessage = null;

            IShellState shellState = MockHelpers.GetMockedShellState(errorMessageCallback: (s) => actualErrorMessage = s);

            HeadCommand command = new HeadCommand();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("HEAD");
            CancellationTokenSource cts = new CancellationTokenSource();

            await command.ExecuteAsync(shellState, httpState, parseResult, cts.Token);

            Assert.Equal(expectedErrorMessage, actualErrorMessage);
        }

        [Fact]
        public async Task ExecuteAsync_WithMultipartRoute_VerifyOutput()
        {
            int expectedResponseLines = 5; // The path, three headers and a blank response.
            string expectedResponseContent = "X-HTTPREPL-TESTHEADER: Header value for HEAD request with route.";
            List<string> actual = new List<string>();

            IShellState shellState = MockHelpers.GetMockedShellState(writeLineCallback: (s) => actual.Add(s));

            HttpState httpState = HttpStateHelpers.Create(_config.BaseAddress, "this/is/a/test/route");

            HeadCommand command = new HeadCommand();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("HEAD");
            CancellationTokenSource cts = new CancellationTokenSource();

            await command.ExecuteAsync(shellState, httpState, parseResult, cts.Token);

            Assert.Equal(expectedResponseLines, actual.Count);
            Assert.Equal(expectedResponseContent, actual[expectedResponseLines - 2]);
        }

        [Fact]
        public async Task ExecuteAsync_WithOnlyBaseAddress_VerifyOutput()
        {
            int expectedResponseLines = 5; // The path, three headers and a blank response.
            string expectedResponseContent = "X-HTTPREPL-TESTHEADER: Header value for root HEAD request.";
            List<string> actual = new List<string>();

            IShellState shellState = MockHelpers.GetMockedShellState(writeLineCallback: (s) => actual.Add(s));

            HttpState httpState = HttpStateHelpers.Create(_config.BaseAddress);

            HeadCommand command = new HeadCommand();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("HEAD");
            CancellationTokenSource cts = new CancellationTokenSource();

            await command.ExecuteAsync(shellState, httpState, parseResult, cts.Token);

            Assert.Equal(expectedResponseLines, actual.Count);
            Assert.Equal(expectedResponseContent, actual[expectedResponseLines - 2]);
        }
    }

    public class HeadCommandsFixture : IDisposable
    {
        private readonly SampleApiServer _testWebServer;
        public HeadCommandsConfig Config { get; } = new HeadCommandsConfig();

        public HeadCommandsFixture()
        {
            _testWebServer = new SampleApiServer(Config);
            _testWebServer.Start();
        }

        public void Dispose()
        {
            _testWebServer.Stop();
        }
    }

    public class HeadCommandsConfig : SampleApiServerConfig
    {
        public HeadCommandsConfig()
        {
            Port = 5053;
            Routes.Add(new DynamicSampleApiServerRoute("HEAD", "", context =>
            {
                context.Response.Headers.Add("X-HTTPREPL-TESTHEADER", "Header value for root HEAD request.");
                return Task.CompletedTask;
            }));
            Routes.Add(new DynamicSampleApiServerRoute("HEAD", "this/is/a/test/route", context =>
            {
                context.Response.Headers.Add("X-HTTPREPL-TESTHEADER", "Header value for HEAD request with route.");
                return Task.CompletedTask;
            }));
        }
    }
}
