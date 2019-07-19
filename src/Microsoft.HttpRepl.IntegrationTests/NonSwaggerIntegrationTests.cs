// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Fakes;
using Microsoft.HttpRepl.IntegrationTests.Commands;
using Microsoft.HttpRepl.IntegrationTests.SampleApi;
using Microsoft.HttpRepl.IntegrationTests.Utilities;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests
{
    public class NonSwaggerIntegrationTests : BaseIntegrationTest, IClassFixture<HttpCommandsFixture<NonSwaggerSampleApiServerConfig>>
    {
        private readonly NonSwaggerSampleApiServerConfig _serverConfig;

        public NonSwaggerIntegrationTests(HttpCommandsFixture<NonSwaggerSampleApiServerConfig> fixture)
        {
            _serverConfig = fixture.Config;
            _serverConfig.EnableSwagger = false;
        }

        [Fact]
        public async Task ListCommand_WithoutSwagger_ShowsNoSubpaths()
        {
            string scriptText = $@"set base {_serverConfig.BaseAddress}
ls
cd api
ls";
            var console = new LoggingConsoleManagerDecorator(new NullConsoleManager());
            using (var script = new TestScript(scriptText))
            {

                await new Program().Start($"run {script.FilePath}".Split(' '), console);
            }

            string output = console.LoggedOutput;
            // remove the first line because it has the randomly generated script file name.
            output = output.Substring(output.IndexOf(Environment.NewLine) + Environment.NewLine.Length);
            output = NormalizeOutput(output, _serverConfig.BaseAddress);

            // make sure to normalize newlines in the expected output
            string expected = NormalizeOutput(@"(Disconnected)~ set base [BaseUrl]

[BaseUrl]/~ ls

[BaseUrl]/~ cd api

[BaseUrl]/api~ ls

[BaseUrl]/api~", null);

            Assert.Equal(expected, output);
        }

        [Fact]
        public async Task ListCommand_WithoutSwagger_ShowsNoActionsOrVerbs()
        {
            string scriptText = $@"set base {_serverConfig.BaseAddress}
cd api/Values
ls";
            var console = new LoggingConsoleManagerDecorator(new NullConsoleManager());
            using (var scriptFile = new TestScript(scriptText))
            {
                await new Program().Start($"run {scriptFile.FilePath}".Split(' '), console);
            }
            string output = console.LoggedOutput;
            // remove the first line because it has the randomly generated script file name.
            output = output.Substring(output.IndexOf(Environment.NewLine) + Environment.NewLine.Length);
            output = NormalizeOutput(output, _serverConfig.BaseAddress);

            string expected = NormalizeOutput(@"(Disconnected)~ set base [BaseUrl]

[BaseUrl]/~ cd api/Values

[BaseUrl]/api/Values~ ls

[BaseUrl]/api/Values~", null);

            Assert.Equal(expected, output);
        }
    }

    public class NonSwaggerSampleApiServerConfig : SampleApiServerConfig
    {
        public NonSwaggerSampleApiServerConfig()
        {
            EnableSwagger = false;
        }
    }
}
