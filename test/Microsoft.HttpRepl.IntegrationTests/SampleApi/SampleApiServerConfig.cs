// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.HttpRepl.IntegrationTests.SampleApi
{
    public class SampleApiServerConfig
    {
        public string BaseAddress => $"http://localhost:{Port}";
        public Lazy<int> Port { get; set; } = new Lazy<int>(() => FindFreeTcpPort());

        /// <summary>
        /// Turns Swagger on or off for the SampleApiServer instance.
        /// </summary>
        public bool EnableSwagger { get; set; } = true;

        public Collection<SampleApiServerRoute> Routes { get; } = new Collection<SampleApiServerRoute>();

        private static int FindFreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
    }

    public abstract class SampleApiServerRoute
    {
        public string Verb { get; set; } = "GET";
        public string Route { get; set; }

        public abstract Task Execute(HttpContext context);
    }

    public sealed class StaticSampleApiServerRoute : SampleApiServerRoute
    {
        public string Result { get; set; }
        public StaticSampleApiServerRoute(string verb, string route, string result)
        {
            Verb = verb;
            Route = route;
            Result = result;
        }

        public async override Task Execute(HttpContext context)
        {
            await context.Response.WriteAsync(Result);
        }
    }

    public sealed class DynamicSampleApiServerRoute : SampleApiServerRoute
    {
        public Func<HttpContext, Task> ResultFunction { get; set; }
        public DynamicSampleApiServerRoute(string verb, string route, Func<HttpContext, Task> resultFunction)
        {
            Verb = verb;
            Route = route;
            ResultFunction = resultFunction;
        }

        public async override Task Execute(HttpContext context)
        {
            await ResultFunction(context);
        }
    }
}
