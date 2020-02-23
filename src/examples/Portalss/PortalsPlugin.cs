using System;
using log4net;
using OpenAPI.Plugins;

namespace Portalss
{
    public class PortalsPlugin : OpenPlugin
    {
	    private static readonly ILog Log = LogManager.GetLogger(typeof(PortalsPlugin));

		public PortalManager PortalManager { get; }
		public PortalsPlugin(OpenAPI.OpenApi api)
	    {
			PortalManager = new PortalManager(api);
	    }

	    public override void Enabled(OpenAPI.OpenApi api)
	    {
			api.PluginManager.SetReference(this);
	    }

	    public override void Disabled(OpenAPI.OpenApi api)
	    {
		    
	    }
    }
}
