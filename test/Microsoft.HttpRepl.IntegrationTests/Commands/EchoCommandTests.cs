// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System.Threading.Tasks;
using Microsoft.HttpRepl.IntegrationTests.SampleApi;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class EchoCommandTests : BaseIntegrationTest, IClassFixture<HttpCommandsFixture<SampleApiServerConfig>>
    {
        private readonly SampleApiServerConfig _serverConfig;

        public EchoCommandTests(HttpCommandsFixture<SampleApiServerConfig> fixture)
        {
            _serverConfig = fixture.Config;
        }

        [Fact]
        public async Task WithEchoOn_ShowsCorrectOutput()
        {
            string scriptText = $@"connect --base {_serverConfig.BaseAddress}
echo on";

            string output = await RunTestScript(scriptText, _serverConfig.BaseAddress);

            string expected = NormalizeOutput(@"(Disconnected)> connect --base [BaseUrl]
Using a base address of [BaseUrl]/
Using OpenAPI description at [BaseUrl]/swagger/v1/swagger.json
For detailed tool info, see https://aka.ms/http-repl-doc

[BaseUrl]/> echo on
Request echoing is on

[BaseUrl]/>", null);

            Assert.Equal(expected, output);
        }

        [Fact]
        public async Task WithEchoOff_ShowsCorrectOutput()
        {
            string scriptText = $@"connect --base {_serverConfig.BaseAddress}
echo off";

            string output = await RunTestScript(scriptText, _serverConfig.BaseAddress);

            string expected = NormalizeOutput(@"(Disconnected)> connect --base [BaseUrl]
Using a base address of [BaseUrl]/
Using OpenAPI description at [BaseUrl]/swagger/v1/swagger.json
For detailed tool info, see https://aka.ms/http-repl-doc

[BaseUrl]/> echo off
Request echoing is off

[BaseUrl]/>", null);

            Assert.Equal(expected, output);
        }
    }
}
