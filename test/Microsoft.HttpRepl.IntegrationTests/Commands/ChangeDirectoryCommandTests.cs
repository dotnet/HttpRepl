// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.HttpRepl.IntegrationTests.SampleApi;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class ChangeDirectoryCommandTests : BaseIntegrationTest, IClassFixture<HttpCommandsFixture<SampleApiServerConfig>>
    {
        private readonly SampleApiServerConfig _serverConfig;

        public ChangeDirectoryCommandTests(HttpCommandsFixture<SampleApiServerConfig> fixture)
        {
            _serverConfig = fixture.Config;
        }

        [Fact]
        public async Task WithSwagger_MethodsAreUppercase()
        {
            string scriptText = $@"connect {_serverConfig.BaseAddress}
ls
cd api
ls
cd Values";
            string output = await RunTestScript(scriptText, _serverConfig.BaseAddress);

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

[BaseUrl]/api> cd Values
/api/Values    [GET|POST]

[BaseUrl]/api/Values>", null);

            Assert.Equal(expected, output);
        }
    }
}
