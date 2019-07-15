// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.Shared.Tests.Commands;
using Microsoft.HttpRepl.Shared.Tests.SampleApi;
using Microsoft.HttpRepl.Tests.Mocks;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class GetCommandTests : HttpCommandTests<GetCommand>, IClassFixture<HttpCommandsFixture<GetCommandsConfig>>
    {
        private readonly GetCommandsConfig _config;
        public GetCommandTests(HttpCommandsFixture<GetCommandsConfig> getCommandsFixture)
            : base(new GetCommand(new MockedFileSystem(), new NullPreferences()))
        {
            _config = getCommandsFixture.Config;
        }

        [Fact]
        public async Task ExecuteAsync_WithNoBasePath_VerifyError()
        {
            await VerifyErrorMessage(commandText: "GET",
                                     baseAddress: null,
                                     path: null,
                                     expectedErrorMessage: Resources.Strings.Error_NoBasePath);
        }

        [Fact]
        public async Task ExecuteAsync_WithMultipartRoute_VerifyOutput()
        {
            await VerifyResponse(commandText: "GET",
                                 baseAddress: _config.BaseAddress,
                                 path: "this/is/a/test/route",
                                 expectedResponseLines: 5,
                                 expectedResponseContent: "This is a test response.");
        }

        [Fact]
        public async Task ExecuteAsync_WithOnlyBaseAddress_VerifyOutput()
        {
            await VerifyResponse(commandText: "GET",
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
            Routes.Add(new StaticSampleApiServerRoute("GET", "", "This is a response from the root."));
            Routes.Add(new StaticSampleApiServerRoute("GET", "this/is/a/test/route", "This is a test response."));
        }
    }
}
