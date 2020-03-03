using System;
using System.IO;
using System.Reflection;
using log4net;
using MiNET;
using MiNET.Plugins;
using MiNET.Worlds;
using Newtonsoft.Json;
using OpenAPI.Commands;
using OpenAPI.Events;
using OpenAPI.Items;
using OpenAPI.Player;
using OpenAPI.Plugins;
using OpenAPI.Utils;
using OpenAPI.Utils.ResourcePacks;
using OpenAPI.World;
using Conf = MiNET.Utils.Config;

namespace OpenAPI
{
	/// <summary>
	/// 	The root for everything happening in OpenApi & it's plugins
	/// </summary>
	public class OpenApi
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(OpenApi));
		
		public OpenItemFactory ItemFactory { get; }
		
		/// <summary>
		/// 	The <see cref="OpenLevelManager"/> instance responsible for all Levels <see cref="OpenLevel"/>
		/// </summary>
		public OpenLevelManager LevelManager { get; }
		
		/// <summary>
		/// 	The root <see cref="OpenPlayerManager"/> responsible for keeping track of all Online Players
		/// </summary>
		public OpenPlayerManager PlayerManager { get; }
		
		/// <summary>
		/// 	The <see cref="OpenMotdProvider"/> responsible for the MOTD's displayed on the client serverlist
		/// </summary>
		public OpenMotdProvider MotdProvider { get; }
		
		/// <summary>
		/// 	The <see cref="OpenPluginManager"/> instance responsible for any loaded plugins
		/// </summary>
		public OpenPluginManager PluginManager { get; }
		
		/// <summary>
		/// 	The root <see cref="EventDispatcher"/>
		/// 	If you want to receive all server wide events <see cref="Event"/> this is the instance to do so.
		/// </summary>
		public EventDispatcher EventDispatcher { get; }
		public CommandManager CommandManager { get; private set; }
		public OpenServerInfo ServerInfo { get; internal set; }
		
		/// <summary>
		/// 	The server instance handling all networking etc
		/// </summary>
	    public OpenServer OpenServer { get; set; }
	    public ResourcePackProvider ResourcePackProvider { get; }

        public OpenApi()
        {
	        JsonConvert.DefaultSettings = () =>
	        {
		        return new JsonSerializerSettings()
		        {
			        MissingMemberHandling = MissingMemberHandling.Ignore
		        };
	        };
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

	        Log.InfoFormat("Enabling OpenAPI...");

	        string pluginDirectoryPaths =
		        Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
	        pluginDirectoryPaths = Conf.GetProperty("PluginDirectory", pluginDirectoryPaths);

	        //foreach (string dirPath in pluginDirectoryPaths.Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries))
	        // {
	        /* string directory = dirPath;
	         if (!Path.IsPathRooted(directory))
	         {
		         directory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), dirPath);
	         }*/

	        PluginManager.DiscoverPlugins(pluginDirectoryPaths.Split(new char[] {';'},
		        StringSplitOptions.RemoveEmptyEntries));
	        // }

	        PluginManager.EnablePlugins();

	        //Only set the default level if it hasn't been set already.
	        if (!LevelManager.HasDefaultLevel)
	        {
		        LevelManager.SetDefaultByConfig();
	        }
	        
	        CommandManager.Init();
        }

        internal void OnDisable()
		{
			PluginManager.UnloadAll();

			ServerInfo.OnDisable();
		}
	}
}
