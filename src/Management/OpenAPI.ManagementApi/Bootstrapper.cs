using System.Collections.Generic;
using System.Linq;
using log4net;
using log4net.Config;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Configuration;
using Nancy.Diagnostics;
using Nancy.ModelBinding;
using Nancy.TinyIoc;
using OpenAPI.ManagementApi.Utils;

namespace OpenAPI.ManagementApi
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Bootstrapper));
        
        private ModuleRegistration[] ModuleRegistrations { get; }
        private ManagementApiPlugin Plugin { get; }
        private OpenAPI Api { get; }
        public Bootstrapper(ManagementApiPlugin plugin, OpenAPI api)
        {
            Plugin = plugin;
            Api = api;
                
            ModuleRegistrations = typeof(ManagementApiPlugin).Assembly.GetTypes().Where(x => typeof(INancyModule).IsAssignableFrom(x))
                .Select(t => new ModuleRegistration(t))
                .ToArray();
        }
        
        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);       
            
            container.Register(typeof(ILog), (c, o) => LogManager.GetLogger(typeof(Bootstrapper)));
            container.Register<OpenAPI>(Api);
            container.Register<ManagementApiPlugin>(Plugin);
            container.Register<MemoryMetricsClient>().AsSingleton();

            container.Register<ISerializer, NewtonJson>();
            container.Register<IBodyDeserializer, JsonNetBodyDeserializer>();
        }

        /// <summary>
        /// Initialise the request - can be used for adding pre/post hooks and
        /// any other per-request initialisation tasks that aren't specifically container setup
        /// related
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="pipelines">Current pipelines</param>
        /// <param name="context">Current context</param>
        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            pipelines.OnError.AddItemToEndOfPipeline((z, a) =>
            {
                Log.Error("Unhandled error on request: " + context.Request.Url + " : " + a.Message, a);
                return a;
            });
            
            base.RequestStartup(container, pipelines, context);
        }

        /// <summary>Configures the Nancy environment</summary>
        /// <param name="environment">The <see cref="T:Nancy.Configuration.INancyEnvironment" /> instance to configure</param>
        public override void Configure(INancyEnvironment environment)
        {
            environment.Tracing(enabled: true, displayErrorTraces: true);
            
            base.Configure(environment);
        }

        protected override IEnumerable<ModuleRegistration> Modules
        {
            get
            {
                return
                    ModuleRegistrations;
            }
        }
    }
}