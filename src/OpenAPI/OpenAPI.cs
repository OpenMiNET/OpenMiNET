using System;
using System.IO;
using System.Reflection;
using log4net;
using MiNET;
using MiNET.Plugins;
using OpenAPI.Events;
using OpenAPI.Items;
using OpenAPI.Player;
using OpenAPI.Plugins;
using OpenAPI.World;
using Conf = MiNET.Utils.Config;

namespace OpenAPI
{
	public class OpenAPI
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(OpenAPI));

		public MiNetServer MiNET { get; private set; }
		public OpenItemFactory ItemFactory { get; }
		public OpenLevelManager LevelManager { get; }
		public OpenPlayerManager PlayerManager { get; }
		public OpenMotdProvider MotdProvider { get; }
		public OpenPluginManager PluginManager { get; }
		public EventDispatcher EventDispatcher { get; }
		public OpenServerInfo ServerInfo { get; internal set; }
	    public OpenServer OpenServer { get; set; }
        public OpenAPI()
	    {
	        ItemFactory = new OpenItemFactory();
	        LevelManager = new OpenLevelManager(this);
	        MotdProvider = new OpenMotdProvider(this);

	        EventDispatcher = new EventDispatcher(this);
	        PlayerManager = new OpenPlayerManager(this);

	        PluginManager = new OpenPluginManager(this);
        }
        
	    internal void OnEnable(OpenServer openServer)
	    {
	        MiNET = openServer;
	        OpenServer = openServer;

			Log.InfoFormat("Enabling OpenAPI...");

		    string pluginDirectoryPaths = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            pluginDirectoryPaths = Conf.GetProperty("PluginDirectory", pluginDirectoryPaths);

		    foreach (string dirPath in pluginDirectoryPaths.Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries))
		    {
			    string directory = dirPath;
			    if (!Path.IsPathRooted(directory))
			    {
				    directory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), dirPath);
			    }

		        PluginManager.DiscoverPlugins(directory);
		    }
		}

		internal void OnDisable()
		{
			PluginManager.UnloadAll();

			ServerInfo.OnDisable();
		}
	}
}
