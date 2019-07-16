// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.Fakes.Commands;
using Microsoft.HttpRepl.Fakes.Mocks;
using Microsoft.HttpRepl.Fakes.SampleApi;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class OptionsCommandTests : HttpCommandTests<OptionsCommand>, IClassFixture<HttpCommandsFixture<OptionsCommandsConfig>>
    {
        private readonly OptionsCommandsConfig _config;
        public OptionsCommandTests(HttpCommandsFixture<OptionsCommandsConfig> optionsCommandsFixture)
            : base(new OptionsCommand(new MockedFileSystem(), new NullPreferences()))
        {
            _config = optionsCommandsFixture.Config;
        }

        [Fact]
        public async Task ExecuteAsync_WithNoBasePath_VerifyError()
        {
            await VerifyErrorMessage(commandText: "OPTIONS",
                                     baseAddress: null,
                                     path: null,
                                     expectedErrorMessage: Resources.Strings.Error_NoBasePath);
        }

        [Fact]
        public async Task ExecuteAsync_WithMultipartRoute_VerifyOutput()
        {
            await VerifyHeaders(commandText: "OPTIONS",
                                baseAddress: _config.BaseAddress,
                                path: "this/is/a/test/route",
                                expectedResponseLines: 6,
                                expectedHeader: "X-HTTPREPL-TESTHEADER: Header value for OPTIONS request with route.");
        }

        [Fact]
        public async Task ExecuteAsync_WithOnlyBaseAddress_VerifyOutput()
        {
            await VerifyHeaders(commandText: "OPTIONS",
                                baseAddress: _config.BaseAddress,
                                path: null,
                                expectedResponseLines: 6,
                                expectedHeader: "X-HTTPREPL-TESTHEADER: Header value for root OPTIONS request.");
        }
    }

    public class OptionsCommandsConfig : SampleApiServerConfig
    {
        public OptionsCommandsConfig()
        {
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
