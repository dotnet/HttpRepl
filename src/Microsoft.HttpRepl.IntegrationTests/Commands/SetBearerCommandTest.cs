using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.HttpRepl.IntegrationTests.SampleApi;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class SetBearerCommandTest : BaseIntegrationTest, IClassFixture<HttpCommandsFixture<SampleApiServerConfig>>
    {

        private readonly SampleApiServerConfig _serverConfig;

        public SetBearerCommandTest(HttpCommandsFixture<SampleApiServerConfig> fixture)
        {
            _serverConfig = fixture.Config;
        }

        [Fact]
        public async Task WithTokenSpecified_SetsBearerToken()
        {
            string scriptText = $@"set base {_serverConfig.BaseAddress}
cd api/authorizedEndpoint
echo on
get bearer
set bearer validToken
get bearer";

            string output = await RunTestScript(scriptText, _serverConfig.BaseAddress);
            string expected = NormalizeOutput(@"(Disconnected)~ set base [BaseUrl]
Using swagger metadata from [BaseUrl]/swagger/v1/swagger.json

[BaseUrl]/~ cd api/authorizedEndpoint
/api/authorizedEndpoint    []

[BaseUrl]/api/authorizedEndpoint~ echo on
Request echoing is on

[BaseUrl]/api/authorizedEndpoint~ get bearer
Request to [BaseUrl]...

GET /api/authorizedEndpoint/bearer HTTP/1.1
User-Agent: HTTP-REPL


Response from [BaseUrl]...

HTTP/1.1 401 Unauthorized
Content-Length: 0
Date: [Date]
Server: Kestrel




[BaseUrl]/api/authorizedEndpoint~ set bearer validToken

[BaseUrl]/api/authorizedEndpoint~ get bearer
Request to [BaseUrl]...

GET /api/authorizedEndpoint/bearer HTTP/1.1
Authorization: bearer validToken
User-Agent: HTTP-REPL


Response from [BaseUrl]...

HTTP/1.1 200 OK
Content-Length: 0
Date: [Date]
Server: Kestrel




[BaseUrl]/api/authorizedEndpoint~", null);
            Assert.Equal(expected, output);

        }



        [Fact]
        public async Task WithNoTokenSpecified_ClearsSetBearerToken()
        {
            string scriptText = $@"set base {_serverConfig.BaseAddress}
cd api/authorizedEndpoint
echo on
get bearer
set bearer validToken
get bearer
set bearer
get bearer";

            string output = await RunTestScript(scriptText, _serverConfig.BaseAddress);
            string expected = NormalizeOutput(@"(Disconnected)~ set base [BaseUrl]
Using swagger metadata from [BaseUrl]/swagger/v1/swagger.json

[BaseUrl]/~ cd api/authorizedEndpoint
/api/authorizedEndpoint    []

[BaseUrl]/api/authorizedEndpoint~ echo on
Request echoing is on

[BaseUrl]/api/authorizedEndpoint~ get bearer
Request to [BaseUrl]...

GET /api/authorizedEndpoint/bearer HTTP/1.1
User-Agent: HTTP-REPL


Response from [BaseUrl]...

HTTP/1.1 401 Unauthorized
Content-Length: 0
Date: [Date]
Server: Kestrel




[BaseUrl]/api/authorizedEndpoint~ set bearer validToken

[BaseUrl]/api/authorizedEndpoint~ get bearer
Request to [BaseUrl]...

GET /api/authorizedEndpoint/bearer HTTP/1.1
Authorization: bearer validToken
User-Agent: HTTP-REPL


Response from [BaseUrl]...

HTTP/1.1 200 OK
Content-Length: 0
Date: [Date]
Server: Kestrel




[BaseUrl]/api/authorizedEndpoint~ set bearer

[BaseUrl]/api/authorizedEndpoint~ get bearer
Request to [BaseUrl]...

GET /api/authorizedEndpoint/bearer HTTP/1.1
Authorization: bearer
User-Agent: HTTP-REPL


Response from [BaseUrl]...

HTTP/1.1 401 Unauthorized
Content-Length: 0
Date: [Date]
Server: Kestrel




[BaseUrl]/api/authorizedEndpoint~", null);
            Assert.Equal(expected, output);

        }

    }
}
