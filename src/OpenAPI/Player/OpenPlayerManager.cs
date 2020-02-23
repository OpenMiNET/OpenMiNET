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
	public class OpenPlayerManager : PlayerFactory, IEventHandler
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(OpenPlayerManager));
		
		private OpenApi _plugin;
		private ConcurrentDictionary<UUID, OpenPlayer> Players { get; } = new ConcurrentDictionary<UUID, OpenPlayer>();

		public new EventHandler<PlayerCreatedEvent> OnPlayerCreated;
		public EventHandler<PlayerJoinEvent> OnPlayerJoin;
		public OpenPlayerManager(OpenApi plugin)
		{
			_plugin = plugin;
			plugin.EventDispatcher.RegisterEvents(this);
		}

	    public OpenPlayer[] GetPlayers()
	    {
	        return Players.Values.ToArray();
	    }

		public bool TryGetPlayer(string name, out OpenPlayer player)
		{
			player = Players.FirstOrDefault(
				x => x.Value.Username.StartsWith(name, StringComparison.InvariantCultureIgnoreCase)).Value;

			if (player == null) return false;
			return true;
		}

		public bool TryGetPlayers(string name, out OpenPlayer[] player)
		{
			player = Players.Where(
				x => x.Value.Username.StartsWith(name, StringComparison.InvariantCultureIgnoreCase)).Select(x => x.Value).ToArray();
			
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
			var player = new OpenPlayer(server, endPoint, _plugin);
			player.ClientUuid = playerInfo.ClientUuid;
			player.MaxViewDistance = Config.GetProperty("MaxViewDistance", 22);
			player.MoveRenderDistance = Config.GetProperty("MoveRenderDistance", 1);

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
