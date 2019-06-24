using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.HttpRepl.Tests.SampleApi;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class HttpCommandsFixture<T> : IDisposable where T : SampleApiServerConfig, new()
    {
        private readonly SampleApiServer _testWebServer;

        public T Config { get; } = new T();

        public HttpCommandsFixture()
        {
            _testWebServer = new SampleApiServer(Config);
            _testWebServer.Start();
        }

        public void Dispose()
        {
            _testWebServer.Stop();
        }
    }
}
