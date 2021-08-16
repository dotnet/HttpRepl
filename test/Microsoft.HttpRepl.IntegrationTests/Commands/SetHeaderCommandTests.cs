// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System.Threading.Tasks;
using Microsoft.HttpRepl.IntegrationTests.SampleApi;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class SetHeaderCommandTests : BaseIntegrationTest, IClassFixture<HttpCommandsFixture<SampleApiServerConfig>>
    {
        private readonly SampleApiServerConfig _serverConfig;

        public SetHeaderCommandTests(HttpCommandsFixture<SampleApiServerConfig> fixture)
        {
            _serverConfig = fixture.Config;
        }

        [Fact]
        public async Task WithNameAndValueSpecified_AddsNewHeaderToListOfHeaders()
        {
            string scriptText = $@"connect {_serverConfig.BaseAddress}
cd api/values
echo on
set header Accept application/json
get";

            string output = await RunTestScript(scriptText, _serverConfig.BaseAddress);

            string expected = NormalizeOutput(@"(Disconnected)> connect [BaseUrl]
Using a base address of [BaseUrl]/
Using OpenAPI description at [BaseUrl]/swagger/v1/swagger.json
For detailed tool info, see https://aka.ms/http-repl-doc

[BaseUrl]/> cd api/values
/api/values    [GET|POST]

[BaseUrl]/api/values> echo on
Request echoing is on

[BaseUrl]/api/values> set header Accept application/json

[BaseUrl]/api/values> get
Request to [BaseUrl]...

GET /api/values HTTP/1.1
Accept: application/json
User-Agent: HTTP-REPL


Response from [BaseUrl]...

HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
Date: [Date]
Server: Kestrel
Transfer-Encoding: chunked

[
  ""value1"",
  ""value2""
]


[BaseUrl]/api/values>", null);

            Assert.Equal(expected, output);
        }

        [Fact]
        public async Task WithEmptyValue_ClearsHeader()
        {
            string scriptText = $@"connect {_serverConfig.BaseAddress}
cd api/values
echo on
set header User-Agent
get";

            string output = await RunTestScript(scriptText, _serverConfig.BaseAddress);

            string expected = NormalizeOutput(@"(Disconnected)> connect [BaseUrl]
Using a base address of [BaseUrl]/
Using OpenAPI description at [BaseUrl]/swagger/v1/swagger.json
For detailed tool info, see https://aka.ms/http-repl-doc

[BaseUrl]/> cd api/values
/api/values    [GET|POST]

[BaseUrl]/api/values> echo on
Request echoing is on

[BaseUrl]/api/values> set header User-Agent

[BaseUrl]/api/values> get
Request to [BaseUrl]...

GET /api/values HTTP/1.1


Response from [BaseUrl]...

HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
Date: [Date]
Server: Kestrel
Transfer-Encoding: chunked

[
  ""value1"",
  ""value2""
]


[BaseUrl]/api/values>", null);

            Assert.Equal(expected, output);
        }
    }
}
