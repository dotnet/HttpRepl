// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

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
Using OpenAPI description at [BaseUrl]/swagger/v1/swagger.json
For detailed tool info, see https://aka.ms/http-repl-doc

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
