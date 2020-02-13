using System;
using log4net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAPI.Plugins;

namespace OpenAPI.ManagementApi
{
    public class ManagementApiPlugin : OpenPlugin
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(OpenPluginManager));
        
        private IHost WebHost { get; set; }
        private OpenAPI Api { get; set; }
        
        public ManagementApiPlugin()
        {
            
        }

        public override void Enabled(OpenAPI api)
        {
            Api = api;
            WebHost = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices(ConfigureServices);
                    webBuilder.Configure(Configure);
                }).Build();

            WebHost.StartAsync().Wait();
        }

        public override void Disabled(OpenAPI api)
        {
            WebHost.StopAsync().Wait();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            
            services.AddSingleton<OpenAPI>(Api);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
        }
    }
}