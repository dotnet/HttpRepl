// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.HttpRepl.IntegrationTests.SampleApi;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class GetCommandTests : BaseIntegrationTest, IClassFixture<HttpCommandsFixture<SampleApiServerConfig>>
    {
        private readonly SampleApiServerConfig _serverConfig;

        public GetCommandTests(HttpCommandsFixture<SampleApiServerConfig> fixture)
        {
            _serverConfig = fixture.Config;
        }

        [Fact]
        public async Task WithoutParameter_ShowsCorrectOutput()
        {
            string scriptText = $@"connect {_serverConfig.BaseAddress}
cd api/values
get";

            string output = await RunTestScript(scriptText, _serverConfig.BaseAddress);

            string expected = NormalizeOutput(@"(Disconnected)> connect [BaseUrl]
Using a base address of [BaseUrl]/
Using swagger definition at [BaseUrl]/swagger/v1/swagger.json

[BaseUrl]/> cd api/values
/api/values    [GET|POST]

[BaseUrl]/api/values> get
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
        public async Task WithParameter_ShowsCorrectOutput()
        {
            string scriptText = $@"connect {_serverConfig.BaseAddress}
cd api/values
get 5";

            string output = await RunTestScript(scriptText, _serverConfig.BaseAddress);

            string expected = NormalizeOutput(@"(Disconnected)> connect [BaseUrl]
Using a base address of [BaseUrl]/
Using swagger definition at [BaseUrl]/swagger/v1/swagger.json

[BaseUrl]/> cd api/values
/api/values    [GET|POST]

[BaseUrl]/api/values> get 5
HTTP/1.1 200 OK
Content-Type: text/plain; charset=utf-8
Date: [Date]
Server: Kestrel
Transfer-Encoding: chunked

value


[BaseUrl]/api/values>", null);

            Assert.Equal(expected, output);
        }

        [Fact]
        public async Task InvalidPath_ShowsNotFoundMessage()
        {
            string scriptText = $@"connect {_serverConfig.BaseAddress}
cd api/invalidpath
get";

            string output = await RunTestScript(scriptText, _serverConfig.BaseAddress);

            string expected = NormalizeOutput(@"(Disconnected)> connect [BaseUrl]
Using a base address of [BaseUrl]/
Using swagger definition at [BaseUrl]/swagger/v1/swagger.json

[BaseUrl]/> cd api/invalidpath
Warning: The '/api/invalidpath' endpoint is not present in the Swagger metadata
/api/invalidpath    []

[BaseUrl]/api/invalidpath> get
HTTP/1.1 404 Not Found
Content-Length: 0
Date: [Date]
Server: Kestrel




[BaseUrl]/api/invalidpath>", null);

            Assert.Equal(expected, output);
        }

        [Fact]
        public async Task WithNoSwaggerAndAbsoluteUrl_ShowsCorrectOutput()
        {
            string scriptText = $@"get {_serverConfig.BaseAddress}/api/values";

            string output = await RunTestScript(scriptText, _serverConfig.BaseAddress);

            string expected = NormalizeOutput(@"(Disconnected)> get [BaseUrl]/api/values
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
Date: [Date]
Server: Kestrel
Transfer-Encoding: chunked

[
  ""value1"",
  ""value2""
]


(Disconnected)>", null);

            Assert.Equal(expected, output);
        }
    }
}
