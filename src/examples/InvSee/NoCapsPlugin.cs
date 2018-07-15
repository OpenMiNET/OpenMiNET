using log4net;
using OpenAPI.Player;
using OpenAPI.Plugins;

namespace NoCaps
{
    public class NoCapsPlugin : OpenPlugin
    {
	    private static readonly ILog Log = LogManager.GetLogger(typeof(NoCapsPlugin));

		private PlayerEventHandler PlayerEventHandler { get; set; }
		public NoCapsPlugin()
	    {
			
	    }

	    public override void Enabled(OpenAPI.OpenAPI api)
	    {
		    PlayerEventHandler = new PlayerEventHandler(api);
			api.EventDispatcher.RegisterEvents(PlayerEventHandler);

		    Log.Info($"ChatFilter plugin enabled!");
		}

	    public override void Disabled(OpenAPI.OpenAPI api)
	    {
		    PlayerEventHandler.CleanUp();

		    Log.Info($"ChatFilter plugin disabled!");
		}
    }
}
