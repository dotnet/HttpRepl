using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.Tests.SampleApi;
using Microsoft.Repl;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class GetCommandTests : IClassFixture<GetCommandsFixture>
    {
        private readonly GetCommandsConfig _config;
        public GetCommandTests(GetCommandsFixture getCommandsFixture)
        {
            _config = getCommandsFixture.Config;
        }

        [Fact]
        public async Task ExecuteAsync_WithNoBasePath_VerifyError()
        {
            HttpState httpState = new HttpState();

            string expectedErrorMessage = Resources.Strings.Error_NoBasePath.SetColor(httpState.ErrorColor);
            string actualErrorMessage = null;

            IShellState shellState = MockHelpers.GetMockedShellState(errorMessageCallback: (s) => actualErrorMessage = s);

            GetCommand command = new GetCommand();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("GET");

            await command.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(expectedErrorMessage, actualErrorMessage);
        }

        [Fact]
        public async Task ExecuteAsync_WithMultipartRoute_VerifyOutput()
        {
            int expectedResponseLines = 5; // The path, three headers and the response.
            string expectedResponseContent = "This is a test response.";
            List<string> actual = new List<string>();
            
            IShellState shellState = MockHelpers.GetMockedShellState(writeLineCallback: (s) => actual.Add(s));

            HttpState httpState = HttpStateHelpers.Create(_config.BaseAddress, "this/is/a/test/route");

            GetCommand command = new GetCommand();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("GET");

            await command.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(expectedResponseLines, actual.Count); 
            Assert.Equal(expectedResponseContent, actual[expectedResponseLines - 1]);
        }

        [Fact]
        public async Task ExecuteAsync_WithOnlyBaseAddress_VerifyOutput()
        {
            int expectedResponseLines = 5; // The path, three headers and the response.
            string expectedResponseContent = "This is a response from the root.";
            List<string> actual = new List<string>();

            IShellState shellState = MockHelpers.GetMockedShellState(writeLineCallback: (s) => actual.Add(s));

            HttpState httpState = HttpStateHelpers.Create(_config.BaseAddress);
            
            GetCommand command = new GetCommand();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("GET");

            await command.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(expectedResponseLines, actual.Count);
            Assert.Equal(expectedResponseContent, actual[expectedResponseLines - 1]);
        }
    }

    public class GetCommandsFixture : IDisposable
    {
        private readonly SampleApiServer _testWebServer;
        public GetCommandsConfig Config { get; } = new GetCommandsConfig();

        public GetCommandsFixture()
        {
            _testWebServer = new SampleApiServer(Config);
            _testWebServer.Start();
        }

        public void Dispose()
        {
            _testWebServer.Stop();
        }
    }

    public class GetCommandsConfig : SampleApiServerConfig
    {
        public GetCommandsConfig()
        {
            Port = SampleApiServerPorts.GetCommandTests;
            Routes.Add(new StaticSampleApiServerRoute("GET", "", "This is a response from the root."));
            Routes.Add(new StaticSampleApiServerRoute("GET", "this/is/a/test/route", "This is a test response."));
            Routes.Add(new DynamicSampleApiServerRoute("GET", "this/is/a/test/route/with/{value}", async (context) =>
            {
                var value = context.GetRouteValue("value");
                await context.Response.WriteAsync($"This is a test response with value {value}.");
            }));
            Routes.Add(new StaticSampleApiServerRoute("POST", "this/is/a/test/route", "This is a test response from a POST."));
        }
    }
}
