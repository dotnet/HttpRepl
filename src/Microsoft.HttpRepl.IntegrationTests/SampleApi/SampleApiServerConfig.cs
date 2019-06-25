using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.HttpRepl.IntegrationTests.SampleApi
{
    public class SampleApiServerConfig
    {
        public string BaseAddress => $"http://localhost:{Port}";
        public int Port { get; protected set; } = 5050;

        public Collection<SampleApiServerRoute> Routes { get; } = new Collection<SampleApiServerRoute>();
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
