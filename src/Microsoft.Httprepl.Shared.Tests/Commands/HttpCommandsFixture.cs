// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.HttpRepl.Shared.Tests.SampleApi;

namespace Microsoft.HttpRepl.Shared.Tests.Commands
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
