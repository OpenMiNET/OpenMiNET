using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using log4net;
using MiNET;
using MiNET.Net;
using MiNET.Utils;
using OpenAPI.Events;
using OpenAPI.Events.Player;

namespace OpenAPI.Player
{
	/// <summary>
	///		Holds all player's connected to the <see cref="OpenServer"/> instance.
	/// </summary>
	public class OpenPlayerManager : PlayerFactory, IEventHandler
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(OpenPlayerManager));
		
		private OpenApi _plugin;
		private ConcurrentDictionary<UUID, OpenPlayer> Players { get; } = new ConcurrentDictionary<UUID, OpenPlayer>();
		
		/// <summary>
		///		Creates a new instance of the OpenPlayerManager
		/// </summary>
		/// <param name="plugin"></param>
		public OpenPlayerManager(OpenApi plugin)
		{
			_plugin = plugin;
			plugin.EventDispatcher.RegisterEvents(this);
		}

		/// <summary>
		///		Get an array of currently connected players
		/// </summary>
		/// <returns>A list of Players</returns>
	    public OpenPlayer[] GetPlayers()
	    {
	        return Players.Values.ToArray();
	    }

		/// <summary>
		///		Get a player by username
		/// </summary>
		/// <param name="name">The username to lookup</param>
		/// <param name="player">If a match was found, returns the best match. Otherwise returns null.</param>
		/// <param name="stringComparison">The string comparison mode to use, defaults to InvariantCultureIgnoreCase</param>
		/// <returns>True if a player was found</returns>
		public bool TryGetPlayer(string name, out OpenPlayer player, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase)
		{
			player = Players.FirstOrDefault(
				x => x.Value.Username.StartsWith(name, stringComparison)).Value;

			if (player == null) return false;
			return true;
		}
		
		/// <summary>
		///		Get's an array of players based on their username
		/// </summary>
		/// <param name="name">The username to lookup</param>
		/// <param name="player">If a match was found, returns a list of matches. Otherwise returns null.</param>
		/// <param name="stringComparison">The string comparison mode to use, defaults to InvariantCultureIgnoreCase</param>
		/// <returns></returns>
		public bool TryGetPlayers(string name, out OpenPlayer[] player, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase)
		{
			player = Players.Where(
				x => x.Value.Username.StartsWith(name, stringComparison)).Select(x => x.Value).ToArray();
			
			if (player.Length == 0) return false;
			return true;
		}

		[EventHandler]
		public void AddPlayer(PlayerJoinEvent e)
		{
			if (Players.TryAdd(e.Player.ClientUuid, e.Player))
			{
				
			}
		}

		[EventHandler]
		public void RemovePlayer(PlayerQuitEvent e)
		{
			if (Players.TryRemove(e.Player.ClientUuid, out OpenPlayer p))
			{
				
			}
		}

		public override MiNET.Player CreatePlayer(MiNetServer server, IPEndPoint endPoint, PlayerInfo playerInfo)
		{
			if (server is not OpenServer openServer)
			{
				return null;
			}

			var player = new OpenPlayer(openServer, endPoint, _plugin)
			{
				ClientUuid = playerInfo.ClientUuid,
				MaxViewDistance = Config.GetProperty("MaxViewDistance", 22),
				MoveRenderDistance = Config.GetProperty("MoveRenderDistance", 1),
				PlayerInfo = playerInfo
			};

			/*	if (!Players.TryAdd(playerInfo.ClientUuid, player))
			{
				Log.Warn("Failed to add player to playermanager!");
			}*/
			//OnPlayerCreated?.Invoke(this, new PlayerCreatedEvent(player));
			_plugin.EventDispatcher.DispatchEvent(new PlayerCreatedEvent(player));

			return player;
		}
	}
}
