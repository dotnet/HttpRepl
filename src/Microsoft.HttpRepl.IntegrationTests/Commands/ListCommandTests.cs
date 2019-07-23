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
            string scriptText = $@"set base {_swaggerServerConfig.BaseAddress}
ls
cd api
ls";
            string output = await RunTestScript(scriptText, _swaggerServerConfig.BaseAddress);

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
Values   [get|post]

[BaseUrl]/api~", null);

            Assert.Equal(expected, output);
        }

        [Fact]
        public async Task WithSwagger_ShowsControllerActionsWithHttpVerbs()
        {
            string scriptText = $@"set base {_swaggerServerConfig.BaseAddress}
cd api/Values
ls";
            string output = await RunTestScript(scriptText, _swaggerServerConfig.BaseAddress);

            string expected = NormalizeOutput(@"(Disconnected)~ set base [BaseUrl]
Using swagger metadata from [BaseUrl]/swagger/v1/swagger.json

[BaseUrl]/~ cd api/Values
/api/Values    [get|post]

[BaseUrl]/api/Values~ ls
.      [get|post]
..     []
{id}   [get|put|delete]

[BaseUrl]/api/Values~", null);

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
            string expected = NormalizeOutput(@"(Disconnected)~ set base [BaseUrl]

[BaseUrl]/~ ls

[BaseUrl]/~ cd api

[BaseUrl]/api~ ls

[BaseUrl]/api~", null);

            Assert.Equal(expected, output);
        }

        [Fact]
        public async Task WithoutSwagger_ShowsNoActionsOrVerbs()
        {
            string scriptText = $@"set base {_nonSwaggerServerConfig.BaseAddress}
cd api/Values
ls";
            string output = await RunTestScript(scriptText, _nonSwaggerServerConfig.BaseAddress);

            string expected = NormalizeOutput(@"(Disconnected)~ set base [BaseUrl]

[BaseUrl]/~ cd api/Values

[BaseUrl]/api/Values~ ls

[BaseUrl]/api/Values~", null);

            Assert.Equal(expected, output);
        }
    }
}
