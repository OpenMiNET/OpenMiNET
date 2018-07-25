using System;
using log4net;
using OpenAPI.Plugins;

namespace Portalss
{
    public class PortalsPlugin : OpenPlugin
    {
	    private static readonly ILog Log = LogManager.GetLogger(typeof(PortalsPlugin));

		public PortalManager PortalManager { get; }
		public PortalsPlugin(OpenAPI.OpenAPI api)
	    {
			PortalManager = new PortalManager(api);
	    }

	    public override void Enabled(OpenAPI.OpenAPI api)
	    {
			api.PluginManager.SetReference(this);
		    Log.Info($"Enabled PortalsPlugin!");
	    }

	    public override void Disabled(OpenAPI.OpenAPI api)
	    {
		    Log.Info($"Disabled PortalsPlugin!");
	    }
    }
}
