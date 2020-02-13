using System;
using log4net;
using Nancy;
using Nancy.Hosting.Self;
using OpenAPI.ManagementApi.Modules;
using OpenAPI.Plugins;

namespace OpenAPI.ManagementApi
{
    [OpenPluginInfo(Name = "OpenAPI Management API", Description = "Provides a rest api that can be used to manage the server", Author = "Kenny van Vulpen", Version = "1.0", Website = "https://github.com/OpenMiNET/OpenAPI")]
    public class ManagementApiPlugin : OpenPlugin
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(OpenPluginManager));
        
        private OpenAPI Api { get; set; }
        private NancyHost Host { get; set; }
        
        public ManagementApiPlugin()
        {
        }

        public override void Enabled(OpenAPI api)
        {
            Api = api;
            
            Host = new NancyHost(new Bootstrapper(this, api), new Uri("http://127.0.0.1:3000"));
            
            Host.Start();

            Log.Info($"Management API Started.");
        }

        private void UnhandledExceptionCallback(Exception obj)
        {
            Log.Error($"Nancy Error: ", obj);
        }

        public override void Disabled(OpenAPI api)
        {
            Host.Stop();
        }
    }
}