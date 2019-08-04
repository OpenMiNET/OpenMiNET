using System;
using System.IO;
using System.Reflection;
using log4net;
using MiNET;
using MiNET.Plugins;
using MiNET.Worlds;
using OpenAPI.Commands;
using OpenAPI.Events;
using OpenAPI.Items;
using OpenAPI.Player;
using OpenAPI.Plugins;
using OpenAPI.Utils;
using OpenAPI.World;
using Conf = MiNET.Utils.Config;

namespace OpenAPI
{
	public class OpenAPI
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(OpenAPI));

		public OpenItemFactory ItemFactory { get; }
		public OpenLevelManager LevelManager { get; }
		public OpenPlayerManager PlayerManager { get; }
		public OpenMotdProvider MotdProvider { get; }
		public OpenPluginManager PluginManager { get; }
		public EventDispatcher EventDispatcher { get; }
		public CommandManager CommandManager { get; private set; }
		public OpenServerInfo ServerInfo { get; internal set; }
	    public OpenServer OpenServer { get; set; }
	    public ResourcePackProvider ResourcePackProvider { get; }

        public OpenAPI()
	    {
	        ItemFactory = new OpenItemFactory();
	        LevelManager = new OpenLevelManager(this);
	        MotdProvider = new OpenMotdProvider(this);

	        EventDispatcher = new EventDispatcher(this);
	        PlayerManager = new OpenPlayerManager(this);

	        PluginManager = new OpenPluginManager(this);
	        CommandManager = new CommandManager(PluginManager);
	        ResourcePackProvider = new ResourcePackProvider(this);
	    }
        
	    internal void OnEnable(OpenServer openServer)
	    {
	        OpenServer = openServer;

	        var lvl = this.LevelManager.GetLevel((MiNET.Player)null, Dimension.Overworld.ToString());
			LevelManager.SetDefaultLevel((OpenLevel)lvl);

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
