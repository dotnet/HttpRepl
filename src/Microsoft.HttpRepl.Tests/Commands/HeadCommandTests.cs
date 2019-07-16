// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.Fakes.Commands;
using Microsoft.HttpRepl.Fakes.SampleApi;
using Microsoft.HttpRepl.Tests.Mocks;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class HeadCommandTests : HttpCommandTests<HeadCommand>, IClassFixture<HttpCommandsFixture<HeadCommandsConfig>>
    {
        private readonly HeadCommandsConfig _config;
        public HeadCommandTests(HttpCommandsFixture<HeadCommandsConfig> headCommandsFixture)
            :  base(new HeadCommand(new MockedFileSystem(), new NullPreferences()))
        {
            _config = headCommandsFixture.Config;
        }

        [Fact]
        public async Task ExecuteAsync_WithNoBasePath_VerifyError()
        {
            await VerifyErrorMessage(commandText: "HEAD",
                                     baseAddress: null,
                                     path: null,
                                     expectedErrorMessage: Resources.Strings.Error_NoBasePath);
        }

        [Fact]
        public async Task ExecuteAsync_WithMultipartRoute_VerifyHeaders()
        {
            await VerifyHeaders(commandText: "HEAD",
                                baseAddress: _config.BaseAddress,
                                path: "this/is/a/test/route",
                                expectedResponseLines: 5,
                                expectedHeader: "X-HTTPREPL-TESTHEADER: Header value for HEAD request with route.");
        }

        [Fact]
        public async Task ExecuteAsync_WithOnlyBaseAddress_VerifyHeaders()
        {
            await VerifyHeaders(commandText: "HEAD",
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
