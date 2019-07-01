using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.HttpRepl.IntegrationTests.SampleApi
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
                                RouteBuilder routeBuilder = new RouteBuilder(app);
                                SetupRoutes(routeBuilder, config);
                                IRouter routes = routeBuilder.Build();
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
            foreach (SampleApiServerRoute route in config.Routes)
            {
                if (route.Verb == "*")
                {
                    routeBuilder.MapRoute(route.Route, context => route.Execute(context));
                }
                else
                {
                    routeBuilder.MapVerb(route.Verb, route.Route, context => route.Execute(context));
                }
            }
        }
    }
}
