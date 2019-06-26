using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.IntegrationTests.SampleApi;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class PatchCommandTests : HttpCommandTests<PatchCommand>, IClassFixture<HttpCommandsFixture<PatchCommandsConfig>>
    {
        private readonly PatchCommandsConfig _config;
        public PatchCommandTests(HttpCommandsFixture<PatchCommandsConfig> PatchCommandsFixture)
        {
            _config = PatchCommandsFixture.Config;
        }

        [Fact]
        public async Task ExecuteAsync_WithNoBasePath_VerifyError()
        {
            await VerifyErrorMessage(commandText: "PATCH",
                                     baseAddress: null,
                                     path: null,
                                     expectedErrorMessage: Resources.Strings.Error_NoBasePath);
        }

        [Fact]
        public async Task ExecuteAsync_OnlyBaseAddressWithInlineContent_VerifyResponse()
        {
            await VerifyResponse(commandText: "PATCH --content \"Test Patch Body\"",
                                 baseAddress: _config.BaseAddress,
                                 path: null,
                                 expectedResponseLines: 5,
                                 expectedResponseContent: "This is a test response from a PATCH: \"Test Patch Body\"");
        }

        [Fact]
        public async Task ExecuteAsync_MultiPartRouteWithInlineContent_VerifyResponse()
        {
            await VerifyResponse(commandText: "PATCH --content \"Test Patch Body\"",
                                 baseAddress: _config.BaseAddress,
                                 path: "this/is/a/test/route",
                                 expectedResponseLines: 5,
                                 expectedResponseContent: "This is a test response from a PATCH: \"Test Patch Body\"");
        }

        [Fact]
        public async Task ExecuteAsync_MultiPartRouteWithNoBodyRequired_VerifyResponse()
        {
            await VerifyResponse(commandText: "PATCH --no-body",
                                 baseAddress: _config.BaseAddress,
                                 path: "no/body/required",
                                 expectedResponseLines: 5,
                                 expectedResponseContent: "This is a test response from a PATCH: \"\"");
        }

        [Fact]
        public async Task ExecuteAsync_MultiPartRouteWithBodyFromFile_VerifyResponse()
        {
            await VerifyResponse(commandText: $"PATCH --file \"{nameof(PatchCommandTests)}-{nameof(ExecuteAsync_MultiPartRouteWithBodyFromFile_VerifyResponse)}.txt\"",
                                 baseAddress: _config.BaseAddress,
                                 path: "this/is/a/test/route",
                                 expectedResponseLines: 5,
                                 expectedResponseContent: "This is a test response from a PATCH: \"Test Patch Body From File\"");
        }
    }

    public class PatchCommandsConfig : SampleApiServerConfig
    {
        public PatchCommandsConfig()
        {
            Port = SampleApiServerPorts.PatchCommandTests;
            Routes.Add(new DynamicSampleApiServerRoute("PATCH", "", RespondWithBody));
            Routes.Add(new DynamicSampleApiServerRoute("PATCH", "this/is/a/test/route", RespondWithBody));
            Routes.Add(new DynamicSampleApiServerRoute("PATCH", "no/body/required", RespondWithBody));
        }

        private async Task RespondWithBody(HttpContext context)
        {
            byte[] buffer = new byte[64];
            int bytesRead = await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
            string body = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            await context.Response.WriteAsync($"This is a test response from a PATCH: \"{body}\"");
        }
    }
}
