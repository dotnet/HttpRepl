// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.HttpRepl.IntegrationTests.SampleApi;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
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

    public class DualHttpCommandsFixture<T> : IDisposable where T : SampleApiServerConfig, new()
    {
        private readonly SampleApiServer _swaggerServer;
        private readonly SampleApiServer _nonSwaggerServer;

        public T SwaggerConfig { get; } = new T();
        public T NonSwaggerConfig { get; } = new T() { EnableSwagger = false };

        public DualHttpCommandsFixture()
        {
            _swaggerServer = new SampleApiServer(SwaggerConfig);
            _swaggerServer.Start();

            _nonSwaggerServer = new SampleApiServer(NonSwaggerConfig);
            _nonSwaggerServer.Start();
        }

        public void Dispose()
        {
            _swaggerServer.Stop();
            _nonSwaggerServer.Stop();
        }
    }
}
