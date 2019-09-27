// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.HttpRepl.IntegrationTests.SampleApi;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class ListCommandTests : BaseIntegrationTest, IClassFixture<DualHttpCommandsFixture<SampleApiServerConfig>>
    {
        private readonly SampleApiServerConfig _swaggerServerConfig;
        private readonly SampleApiServerConfig _nonSwaggerServerConfig;

        public ListCommandTests(DualHttpCommandsFixture<SampleApiServerConfig> fixture)
        {
            _swaggerServerConfig = fixture.SwaggerConfig;
            _nonSwaggerServerConfig = fixture.NonSwaggerConfig;
        }

        [Fact]
        public async Task WithSwagger_ShowsAvailableSubpaths()
        {
            string scriptText = $@"connect {_swaggerServerConfig.BaseAddress}
ls
cd api
ls";
            string output = await RunTestScript(scriptText, _swaggerServerConfig.BaseAddress);

            // make sure to normalize newlines in the expected output
            string expected = NormalizeOutput(@"(Disconnected)> connect [BaseUrl]
Using a base address of [BaseUrl]/
Using swagger definition at [BaseUrl]/swagger/v1/swagger.json

[BaseUrl]/> ls
.     []
api   []

[BaseUrl]/> cd api
/api    []

[BaseUrl]/api> ls
.        []
..       []
Values   [GET|POST]

[BaseUrl]/api>", null);

            Assert.Equal(expected, output);
        }

        [Fact]
        public async Task WithSwagger_ShowsControllerActionsWithHttpVerbs()
        {
            string scriptText = $@"connect {_swaggerServerConfig.BaseAddress}
cd api/Values
ls";
            string output = await RunTestScript(scriptText, _swaggerServerConfig.BaseAddress);

            string expected = NormalizeOutput(@"(Disconnected)> connect [BaseUrl]
Using a base address of [BaseUrl]/
Using swagger definition at [BaseUrl]/swagger/v1/swagger.json

[BaseUrl]/> cd api/Values
/api/Values    [GET|POST]

[BaseUrl]/api/Values> ls
.      [GET|POST]
..     []
{id}   [GET|PUT|DELETE]

[BaseUrl]/api/Values>", null);

            Assert.Equal(expected, output);
        }

        [Fact]
        public async Task WithoutSwagger_ShowsNoSubpaths()
        {
            string scriptText = $@"set base {_nonSwaggerServerConfig.BaseAddress}
ls
cd api
ls";
            string output = await RunTestScript(scriptText, _nonSwaggerServerConfig.BaseAddress);

            // make sure to normalize newlines in the expected output
            string expected = NormalizeOutput(@"(Disconnected)> set base [BaseUrl]

[BaseUrl]/> ls
No directory structure has been set, so there is nothing to list. Use the ""set swagger"" command to set a directory structure based on a swagger definition.

[BaseUrl]/> cd api

[BaseUrl]/api> ls
No directory structure has been set, so there is nothing to list. Use the ""set swagger"" command to set a directory structure based on a swagger definition.

[BaseUrl]/api>", null);

            Assert.Equal(expected, output);
        }

        [Fact]
        public async Task WithoutSwagger_ShowsNoActionsOrVerbs()
        {
            string scriptText = $@"set base {_nonSwaggerServerConfig.BaseAddress}
cd api/Values
ls";
            string output = await RunTestScript(scriptText, _nonSwaggerServerConfig.BaseAddress);

            string expected = NormalizeOutput(@"(Disconnected)> set base [BaseUrl]

[BaseUrl]/> cd api/Values

[BaseUrl]/api/Values> ls
No directory structure has been set, so there is nothing to list. Use the ""set swagger"" command to set a directory structure based on a swagger definition.

[BaseUrl]/api/Values>", null);

            Assert.Equal(expected, output);
        }
    }
}
