using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.HttpRepl.Tests.SampleApi
{
    public class SampleApiServer
    {
        private readonly IHost _Host;
        public SampleApiServer(SampleApiServerConfig config)
        {
            _Host = Host.CreateDefaultBuilder()
                        .ConfigureWebHostDefaults(webBuilder =>
                        {
                            webBuilder.UseUrls(config.BaseAddress);
                            webBuilder.Configure(app =>
                            {
                                app.UseDeveloperExceptionPage();
                            
                                app.UseRouting();
                                var routeBuilder = new RouteBuilder(app);
                                SetupRoutes(routeBuilder, config);
                                var routes = routeBuilder.Build();
                                app.UseRouter(routes);
                            });
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
