using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.IntegrationTests.SampleApi;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class GetCommandTests : BaseHttpCommandTests, IClassFixture<HttpCommandsFixture<GetCommandsConfig>>
    {
        private readonly GetCommandsConfig _config;
        public GetCommandTests(HttpCommandsFixture<GetCommandsConfig> getCommandsFixture)
        {
            _config = getCommandsFixture.Config;
        }

        [Fact]
        public async Task ExecuteAsync_WithNoBasePath_VerifyError()
        {
            await VerifyErrorMessage(command: new GetCommand(),
                                     commandText: "GET",
                                     baseAddress: null,
                                     path: null,
                                     expectedErrorMessage: Resources.Strings.Error_NoBasePath);
        }

        [Fact]
        public async Task ExecuteAsync_WithMultipartRoute_VerifyOutput()
        {
            await VerifyResponse(command: new GetCommand(),
                                 commandText: "GET",
                                 baseAddress: _config.BaseAddress,
                                 path: "this/is/a/test/route",
                                 expectedResponseLines: 5,
                                 expectedResponseContent: "This is a test response.");
        }

        [Fact]
        public async Task ExecuteAsync_WithOnlyBaseAddress_VerifyOutput()
        {
            await VerifyResponse(command: new GetCommand(),
                                 commandText: "GET",
                                 baseAddress: _config.BaseAddress,
                                 path: null,
                                 expectedResponseLines: 5,
                                 expectedResponseContent: "This is a response from the root.");
        }
    }

    public class GetCommandsConfig : SampleApiServerConfig
    {
        public GetCommandsConfig()
        {
            Port = SampleApiServerPorts.GetCommandTests;
            Routes.Add(new StaticSampleApiServerRoute("GET", "", "This is a response from the root."));
            Routes.Add(new StaticSampleApiServerRoute("GET", "this/is/a/test/route", "This is a test response."));
        }
    }
}
