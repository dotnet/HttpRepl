// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
            string scriptText = $@"set base {_serverConfig.BaseAddress}
echo on";

            string output = await RunTestScript(scriptText, _serverConfig.BaseAddress);

            string expected = NormalizeOutput(@"(Disconnected)> set base [BaseUrl]

[BaseUrl]/> echo on
Request echoing is on

[BaseUrl]/>", null);

            Assert.Equal(expected, output);
        }

        [Fact]
        public async Task WithEchoOff_ShowsCorrectOutput()
        {
            string scriptText = $@"set base {_serverConfig.BaseAddress}
echo off";

            string output = await RunTestScript(scriptText, _serverConfig.BaseAddress);

            string expected = NormalizeOutput(@"(Disconnected)> set base [BaseUrl]

[BaseUrl]/> echo off
Request echoing is off

[BaseUrl]/>", null);

            Assert.Equal(expected, output);
        }
    }
}
