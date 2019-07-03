// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.HttpRepl.IntegrationTests.Commands;
using Microsoft.HttpRepl.IntegrationTests.Mocks;
using Microsoft.HttpRepl.IntegrationTests.SampleApi;
using Microsoft.HttpRepl.IntegrationTests.Utilities;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests
{
    public class SwaggerIntegrationTests : IClassFixture<HttpCommandsFixture<SampleApiServerConfig>>
    {
        private readonly HttpCommandsFixture<SampleApiServerConfig> _serverFixture;

        public SwaggerIntegrationTests(HttpCommandsFixture<SampleApiServerConfig> fixture)
        {
            _serverFixture = fixture;
        }
        
        [Fact]
        public async Task ListCommand_WithSwagger_ShowsAvailableSubpaths()
        {
            string scriptText = $@"set base {_serverFixture.Config.BaseAddress}
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
            output = NormalizeOutput(output, _serverFixture.Config.BaseAddress);

            // make sure to normalize newlines in the expected output
            string expected = NormalizeOutput(@"(Disconnected)~ set base [BaseUrl]
Using swagger metadata from [BaseUrl]/swagger/v1/swagger.json

[BaseUrl]/~ ls
.     []
api   []

[BaseUrl]/~ cd api
/api    []

[BaseUrl]/api~ ls
.        []
..       []
Values   [post]

[BaseUrl]/api~", null);

            Assert.Equal(expected, output);
        }

        [Fact]
        public async Task ListCommand_WithSwagger_ShowsControllerActionsWithHttpVerbs()
        {
            string scriptText = $@"set base {_serverFixture.Config.BaseAddress}
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
            output = NormalizeOutput(output, _serverFixture.Config.BaseAddress);

            string expected = NormalizeOutput(@"(Disconnected)~ set base [BaseUrl]
Using swagger metadata from [BaseUrl]/swagger/v1/swagger.json

[BaseUrl]/~ cd api/Values
/api/Values    [post]

[BaseUrl]/api/Values~ ls
.      [post]
..     []
{id}   [put]

[BaseUrl]/api/Values~", null);

            Assert.Equal(expected, output);
        }

        // TODO: at some point we should probably have a base class for integration tests, and this should be moved there.
        private static string NormalizeOutput(string output, string baseUrl)
        {
            // The console implementation uses trailing whitespace when a new line's text is shorter than the previous
            // line.  For example (the trailing * represent spaces):
            // Line 1: (Disconnected)~ run C:\path\to\a\test\script\file.txt
            // Line 2: (Disconnected)~ set base http://localhost:12345******
            // This having this whitespace makes it harder to read/write test baselines, so here we'll trim each line
            string result = string.Join(Environment.NewLine, output.Split(Environment.NewLine).Select(l => l.TrimEnd()));

            // next, normalize the base URL from the test fixture
            if (!string.IsNullOrEmpty(baseUrl))
            {
                result = result.Replace(baseUrl, "[BaseUrl]");
            }

            return result;
        }
    }
}
