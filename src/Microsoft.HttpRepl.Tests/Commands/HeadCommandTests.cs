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
    public class HeadCommandTests : BaseHttpCommandTests, IClassFixture<HttpCommandsFixture<HeadCommandsConfig>>
    {
        private readonly HeadCommandsConfig _config;
        public HeadCommandTests(HttpCommandsFixture<HeadCommandsConfig> headCommandsFixture)
        {
            _config = headCommandsFixture.Config;
        }

        [Fact]
        public async Task ExecuteAsync_WithNoBasePath_VerifyError()
        {
            await VerifyErrorMessage(command: new HeadCommand(),
                                     commandText: "HEAD",
                                     baseAddress: null,
                                     path: null,
                                     expectedErrorMessage: Resources.Strings.Error_NoBasePath);
        }

        [Fact]
        public async Task ExecuteAsync_WithMultipartRoute_VerifyHeaders()
        {
            await VerifyHeaders(command: new HeadCommand(),
                                commandText: "HEAD",
                                baseAddress: _config.BaseAddress,
                                path: "this/is/a/test/route",
                                expectedResponseLines: 5,
                                expectedHeader: "X-HTTPREPL-TESTHEADER: Header value for HEAD request with route.");
        }

        [Fact]
        public async Task ExecuteAsync_WithOnlyBaseAddress_VerifyHeaders()
        {
            await VerifyHeaders(command: new HeadCommand(),
                                commandText: "HEAD",
                                baseAddress: _config.BaseAddress,
                                path: null,
                                expectedResponseLines: 5,
                                expectedHeader: "X-HTTPREPL-TESTHEADER: Header value for root HEAD request.");
        }
    }

    public class HeadCommandsConfig : SampleApiServerConfig
    {
        public HeadCommandsConfig()
        {
            Port = SampleApiServerPorts.HeadCommandTests;
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
