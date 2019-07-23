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
        public async Task WithoutParameter_ShowsColorizedOutput()
        {
            string scriptText = $@"set base {_serverConfig.BaseAddress}
cd api/values
get";

            string output = await RunTestScript(scriptText, _serverConfig.BaseAddress);

            string expected = NormalizeOutput(@"(Disconnected)~ set base [BaseUrl]
Using swagger metadata from [BaseUrl]/swagger/v1/swagger.json

[BaseUrl]/~ cd api/values
/api/values    [get|post]

[BaseUrl]/api/values~ get
[32m[1mHTTP[22m[39m[32m[1m/[22m[39m[32m[1m1.1[22m[39m [33m[1m200[22m[39m [33m[1mOK[22m[39m
Content-Type: application/json; charset=utf-8
Date: [Date]
Server: Kestrel
Transfer-Encoding: chunked

[36m[1m[[22m[39m
  [32m""value1""[39m[33m[1m,[22m[39m
  [32m""value2""[39m
[36m[1m][22m[39m


[BaseUrl]/api/values~", null);

            Assert.Equal(expected, output);
        }

        [Fact]
        public async Task WithParameter_ShowsColorizedOutput()
        {
            string scriptText = $@"set base {_serverConfig.BaseAddress}
cd api/values
get 5";

            string output = await RunTestScript(scriptText, _serverConfig.BaseAddress);

            string expected = NormalizeOutput(@"(Disconnected)~ set base [BaseUrl]
Using swagger metadata from [BaseUrl]/swagger/v1/swagger.json

[BaseUrl]/~ cd api/values
/api/values    [get|post]

[BaseUrl]/api/values~ get 5
[32m[1mHTTP[22m[39m[32m[1m/[22m[39m[32m[1m1.1[22m[39m [33m[1m200[22m[39m [33m[1mOK[22m[39m
Content-Type: text/plain; charset=utf-8
Date: [Date]
Server: Kestrel
Transfer-Encoding: chunked

value


[BaseUrl]/api/values~", null);

            Assert.Equal(expected, output);
        }

        [Fact]
        public async Task InvalidPath_ShowsNotFoundMessage()
        {
            string scriptText = $@"set base {_serverConfig.BaseAddress}
cd api/invalidpath
get";

            string output = await RunTestScript(scriptText, _serverConfig.BaseAddress);

            string expected = NormalizeOutput(@"(Disconnected)~ set base [BaseUrl]
Using swagger metadata from [BaseUrl]/swagger/v1/swagger.json

[BaseUrl]/~ cd api/invalidpath
[33m[1mWarning: The '/api/invalidpath' endpoint is not present in the Swagger metadata[22m[39m
/api/invalidpath    []

[BaseUrl]/api/invalidpath~ get
[32m[1mHTTP[22m[39m[32m[1m/[22m[39m[32m[1m1.1[22m[39m [33m[1m404[22m[39m [33m[1mNot Found[22m[39m
Content-Length: 0
Date: [Date]
Server: Kestrel




[BaseUrl]/api/invalidpath~", null);

            Assert.Equal(expected, output);
        }

        [Fact]
        public async Task WithNoSwaggerAndAbsoluteUrl_ShowsColorizedOutput()
        {
            string scriptText = $@"get {_serverConfig.BaseAddress}/api/values";

            string output = await RunTestScript(scriptText, _serverConfig.BaseAddress);

            string expected = NormalizeOutput(@"(Disconnected)~ get [BaseUrl]/api/values
[32m[1mHTTP[22m[39m[32m[1m/[22m[39m[32m[1m1.1[22m[39m [33m[1m200[22m[39m [33m[1mOK[22m[39m
Content-Type: application/json; charset=utf-8
Date: [Date]
Server: Kestrel
Transfer-Encoding: chunked

[36m[1m[[22m[39m
  [32m""value1""[39m[33m[1m,[22m[39m
  [32m""value2""[39m
[36m[1m][22m[39m


(Disconnected)~", null);

            Assert.Equal(expected, output);
        }
    }
}
