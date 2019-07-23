// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Microsoft.HttpRepl.IntegrationTests.SampleApi
{
    public class SampleApiServer
    {
        private readonly IWebHost _Host;
        public SampleApiServer(SampleApiServerConfig config)
        {
            _Host = WebHost.CreateDefaultBuilder()
                           .UseKestrel(options => options.ListenLocalhost(config.Port.Value))
                           .ConfigureServices(services =>
                           {
                               services.AddControllers();
                               if (config.EnableSwagger)
                               {
                                   services.AddSwaggerGen(c =>
                                   {
                                       c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
                                   });
                               }
                           })
                           .Configure(app =>
                           {
                               app.UseDeveloperExceptionPage();

                               app.UseRouting();

                               app.UseEndpoints(endpoints =>
                               {
                                   endpoints.MapControllers();
                               });

                               if (config.EnableSwagger)
                               {
                                   app.UseSwagger();
                               }
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
    }
}
