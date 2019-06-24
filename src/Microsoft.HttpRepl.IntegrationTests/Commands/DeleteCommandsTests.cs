using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.IntegrationTests.SampleApi;
using Microsoft.Repl;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class DeleteCommandTests : BaseHttpCommandTests, IClassFixture<HttpCommandsFixture<DeleteCommandsConfig>>
    {
        private readonly DeleteCommandsConfig _config;
        public DeleteCommandTests(HttpCommandsFixture<DeleteCommandsConfig> deleteCommandsFixture)
        {
            _config = deleteCommandsFixture.Config;
        }

        [Fact]
        public async Task ExecuteAsync_WithNoBasePath_VerifyError()
        {
            await VerifyErrorMessage(command: new DeleteCommand(),
                                     commandText: "DELETE",
                                     baseAddress: null,
                                     path: null,
                                     expectedErrorMessage: Resources.Strings.Error_NoBasePath);
        }

        [Fact]
        public async Task ExecuteAsync_WithMultipartRoute_VerifyOutput()
        {
            await VerifyResponse(command: new DeleteCommand(),
                                 commandText: "DELETE",
                                 baseAddress: _config.BaseAddress,
                                 path: "a/file/path.txt",
                                 expectedResponseLines: 5,
                                 expectedResponseContent: "File path delete received successfully.");
        }

        [Fact]
        public async Task ExecuteAsync_WithOnlyBaseAddress_VerifyOutput()
        {
            await VerifyResponse(command: new DeleteCommand(),
                                 commandText: "DELETE",
                                 baseAddress: _config.BaseAddress,
                                 path: null,
                                 expectedResponseLines: 5,
                                 expectedResponseContent: "Root delete received successfully.");
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
