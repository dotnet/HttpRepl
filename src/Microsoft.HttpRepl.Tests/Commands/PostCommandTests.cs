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
    public class PostCommandTests : BaseHttpCommandTests, IClassFixture<HttpCommandsFixture<PostCommandsConfig>>
    {
        private readonly PostCommandsConfig _config;
        public PostCommandTests(HttpCommandsFixture<PostCommandsConfig> postCommandsFixture)
        {
            _config = postCommandsFixture.Config;
        }

        [Fact]
        public async Task ExecuteAsync_WithNoBasePath_VerifyError()
        {
            await VerifyErrorMessage(command: new PostCommand(),
                                     commandText: "POST",
                                     baseAddress: null,
                                     path: null,
                                     expectedErrorMessage: Resources.Strings.Error_NoBasePath);
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
