using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.HttpRepl.Tests.SampleApi
{
    public class SampleApiServer
    {
        private readonly IWebHost _Host;
        public SampleApiServer(SampleApiServerConfig config)
        {
            _Host = WebHost.CreateDefaultBuilder()
                           .UseUrls(config.BaseAddress)
                           .Configure(app =>
                           {
                               app.UseDeveloperExceptionPage();

                               var routeBuilder = new RouteBuilder(app);
                               SetupRoutes(routeBuilder, config);
                               var routes = routeBuilder.Build();
                               app.UseRouter(routes);
                           })
                           .ConfigureServices(configureServices =>
                           {
                               configureServices.AddRouting();
                           })
                           .Build();
        }

        public void Start()
        {
            _Host.RunAsync();
        }

        public void Stop()
        {
            _Host.StopAsync();
        }

        private static void SetupRoutes(RouteBuilder routeBuilder, SampleApiServerConfig config)
        {
            foreach (var route in config.Routes)
            {
                switch (route.Verb)
                {
                    case "*":
                        routeBuilder.MapRoute(route.Route, context => route.Execute(context));
                        break;
                    case "GET":
                        routeBuilder.MapGet(route.Route, context => route.Execute(context));
                        break;
                    case "POST":
                        routeBuilder.MapPost(route.Route, context => route.Execute(context));
                        break;
                }
            }
        }
    }
}
