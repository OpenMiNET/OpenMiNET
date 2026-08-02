using System;
using System.Collections.Generic;
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
		
		/// <summary>
		///		The <see cref="OpenItemFactory"/> instance used globally
		/// </summary>
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
		
		/// <summary>
		///		The root <see cref="CommandManager"/>
		///		Responsible for managing & executing all player commands
		/// </summary>
		public CommandManager CommandManager { get; }
		
		/// <summary>
		///		The serverinfo, contains information like the playercount & MOTD.
		/// </summary>
		public OpenServerInfo ServerInfo { get; internal set; }
		
		/// <summary>
		/// 	The server instance handling all networking etc
		/// </summary>
	    public OpenServer OpenServer { get; private set; }
		
		/// <summary>
		///		Manages the resourcepacks sent to players
		/// </summary>
	    public ResourcePackProvider ResourcePackProvider { get; }

		/// <summary>
		///		Everything that can hold a reference into a plugin assembly and must release it
		///		on unload — every <see cref="EventDispatcher"/> and every
		///		<see cref="Utils.TickScheduler"/>, which means one per level as well as the root.
		/// </summary>
		/// <remarks>
		///		Weak, because levels own these: a strong list would keep every level that has
		///		ever existed alive.
		///
		///		They self-register rather than being walked from the
		///		<see cref="World.OpenLevelManager"/>, so a level that was constructed but never
		///		registered is still reachable for teardown.
		///
		///		Initialised inline: the root dispatcher is constructed in this type's constructor
		///		and registers itself, so this must already exist by then.
		/// </remarks>
		private readonly List<WeakReference<IAssemblyPurgeable>> _purgeables =
			new List<WeakReference<IAssemblyPurgeable>>();

		/// <summary>
		///		Registers an object that must release its plugin references on unload.
		/// </summary>
		public void RegisterPurgeable(IAssemblyPurgeable purgeable)
		{
			if (purgeable == null) return;

			lock (_purgeables)
			{
				_purgeables.RemoveAll(x => !x.TryGetTarget(out _));
				_purgeables.Add(new WeakReference<IAssemblyPurgeable>(purgeable));
			}
		}

		/// <summary>
		///		Returns everything still alive, pruning what has been collected.
		/// </summary>
		internal IAssemblyPurgeable[] GetPurgeables()
		{
			lock (_purgeables)
			{
				_purgeables.RemoveAll(x => !x.TryGetTarget(out _));

				var result = new List<IAssemblyPurgeable>(_purgeables.Count);
				foreach (var reference in _purgeables)
				{
					if (reference.TryGetTarget(out var purgeable))
						result.Add(purgeable);
				}

				return result.ToArray();
			}
		}

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

	        string pluginDirectoryPaths = AppContext.BaseDirectory;
	        pluginDirectoryPaths = Conf.GetProperty("PluginDirectory", pluginDirectoryPaths);

	        PluginManager.DiscoverPlugins(pluginDirectoryPaths.Split(new char[] {';'},
		        StringSplitOptions.RemoveEmptyEntries));

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
			
			LevelManager.Close();
		}
	}
}
