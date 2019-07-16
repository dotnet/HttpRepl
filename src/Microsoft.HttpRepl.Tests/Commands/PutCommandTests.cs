// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.Fakes.Commands;
using Microsoft.HttpRepl.Fakes.Mocks;
using Microsoft.HttpRepl.Fakes.SampleApi;
using Microsoft.HttpRepl.FileSystem;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class PutCommandTests : HttpCommandTests<PutCommand>, IClassFixture<HttpCommandsFixture<PutCommandsConfig>>
    {
        private static readonly IFileSystem _fileSystem = new MockedFileSystem().AddFile($"{nameof(ExecuteAsync_MultiPartRouteWithBodyFromFile_VerifyResponse)}.txt", "Test Put Body From File");

        private readonly PutCommandsConfig _config;
        public PutCommandTests(HttpCommandsFixture<PutCommandsConfig> PutCommandsFixture)
            : base(new PutCommand(_fileSystem, new NullPreferences()))
        {
            _config = PutCommandsFixture.Config;
        }

        [Fact]
        public async Task ExecuteAsync_WithNoBasePath_VerifyError()
        {
            await VerifyErrorMessage(commandText: "PUT",
                                     baseAddress: null,
                                     path: null,
                                     expectedErrorMessage: Resources.Strings.Error_NoBasePath);
        }

        [Fact]
        public async Task ExecuteAsync_OnlyBaseAddressWithInlineContent_VerifyResponse()
        {
            await VerifyResponse(commandText: "PUT --content \"Test Put Body\"",
                                 baseAddress: _config.BaseAddress,
                                 path: null,
                                 expectedResponseLines: 5,
                                 expectedResponseContent: "This is a test response from a PUT: \"Test Put Body\"");
        }

        [Fact]
        public async Task ExecuteAsync_MultiPartRouteWithInlineContent_VerifyResponse()
        {
            await VerifyResponse(commandText: "PUT --content \"Test Put Body\"",
                                 baseAddress: _config.BaseAddress,
                                 path: "this/is/a/test/route",
                                 expectedResponseLines: 5,
                                 expectedResponseContent: "This is a test response from a PUT: \"Test Put Body\"");
        }

        [Fact]
        public async Task ExecuteAsync_MultiPartRouteWithNoBodyRequired_VerifyResponse()
        {
            await VerifyResponse(commandText: "PUT --no-body",
                                 baseAddress: _config.BaseAddress,
                                 path: "no/body/required",
                                 expectedResponseLines: 5,
                                 expectedResponseContent: "This is a test response from a PUT: \"\"");
        }

        [Fact]
        public async Task ExecuteAsync_MultiPartRouteWithBodyFromFile_VerifyResponse()
        {
            await VerifyResponse(commandText: $"PUT --file \"{nameof(ExecuteAsync_MultiPartRouteWithBodyFromFile_VerifyResponse)}.txt\"",
                                 baseAddress: _config.BaseAddress,
                                 path: "this/is/a/test/route",
                                 expectedResponseLines: 5,
                                 expectedResponseContent: "This is a test response from a PUT: \"Test Put Body From File\"");
        }
    }

    public class PutCommandsConfig : SampleApiServerConfig
    {
        public PutCommandsConfig()
        {
            Routes.Add(new DynamicSampleApiServerRoute("PUT", "", RespondWithBody));
            Routes.Add(new DynamicSampleApiServerRoute("PUT", "this/is/a/test/route", RespondWithBody));
            Routes.Add(new DynamicSampleApiServerRoute("PUT", "no/body/required", RespondWithBody));
        }

        private async Task RespondWithBody(HttpContext context)
        {
            byte[] buffer = new byte[64];
            int bytesRead = await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
            string body = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            await context.Response.WriteAsync($"This is a test response from a PUT: \"{body}\"");
        }
    }
}
