using System.IO;
using System.Reflection;
using log4net;
using OpenAPI.Plugins;

namespace ChatFilter
{
	[OpenPluginInfo(Name = "OpenAPI ChatFilter", Description = "An example plugin show chat filtering abilities", Author = "Kenny van Vulpen", Version = "1.0", Website = "https://github.com/OpenMiNET/OpenAPI")]
    public class NoCapsPlugin : OpenPlugin
    {
	    private static readonly ILog Log = LogManager.GetLogger(typeof(NoCapsPlugin));

	    public static string PluginDirectory =
		    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ChatFilter");

		private PlayerEventHandler PlayerEventHandler { get; set; }
		public NoCapsPlugin()
		{
			if (!Directory.Exists(PluginDirectory))
				Directory.CreateDirectory(PluginDirectory);
		}

	    public override void Enabled(OpenAPI.OpenApi api)
	    {
		    PlayerEventHandler = new PlayerEventHandler(api);
			api.EventDispatcher.RegisterEvents(PlayerEventHandler);

		    Log.Info($"ChatFilter plugin enabled!");
		}

	    public override void Disabled(OpenAPI.OpenApi api)
	    {
		    PlayerEventHandler.CleanUp();

		    Log.Info($"ChatFilter plugin disabled!");
		}
    }
}
