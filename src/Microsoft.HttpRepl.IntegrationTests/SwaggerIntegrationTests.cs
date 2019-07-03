// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.HttpRepl.IntegrationTests.Commands;
using Microsoft.HttpRepl.IntegrationTests.Mocks;
using Microsoft.HttpRepl.IntegrationTests.SampleApi;
using Microsoft.HttpRepl.IntegrationTests.Utilities;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests
{
    public class SwaggerIntegrationTests : IClassFixture<HttpCommandsFixture<SwaggerEndpointTestsConfig>>
    {
        [Fact]
        public async Task ListCommand_WithSwagger_ShowsAvailableSubpaths()
        {
            string scriptText = $@"set base http://localhost:12345
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

            string expected = @"(Disconnected)~ set base http://localhost:12345                   
Using swagger metadata from http://localhost:12345/swagger/v1/swagger.json

http://localhost:12345/~ ls
.     []
api   []

http://localhost:12345/~ cd api
/api    []

http://localhost:12345/api~ ls
.        []
..       []
Values   [post]

http://localhost:12345/api~ ";

            Assert.Equal(expected, output);
        }

        [Fact]
        public async Task ListCommand_WithSwagger_ShowsControllerActionsWithHttpVerbs()
        {
            string scriptText = $@"set base http://localhost:12345
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

            string expected = @"(Disconnected)~ set base http://localhost:12345                   
Using swagger metadata from http://localhost:12345/swagger/v1/swagger.json

http://localhost:12345/~ cd api/Values
/api/Values    [post]

http://localhost:12345/api/Values~ ls
.      [post]
..     []
{id}   [put]

http://localhost:12345/api/Values~ ";

            Assert.Equal(expected, output);
        }
    }

    public class SwaggerEndpointTestsConfig : SampleApiServerConfig
    {
        public SwaggerEndpointTestsConfig()
        {
            Port = 12345;
        }
    }
}
