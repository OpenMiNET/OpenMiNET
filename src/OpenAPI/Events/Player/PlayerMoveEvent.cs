using MiNET.Utils;
using OpenAPI.Player;

namespace OpenAPI.Events.Player
{
	public class PlayerMoveEvent : PlayerEvent
	{
		public PlayerLocation From { get; }
		public PlayerLocation To { get; }
		public bool IsTeleport { get; }
		public PlayerMoveEvent(OpenPlayer player, PlayerLocation from, PlayerLocation to, bool teleport) : base(player)
		{
			From = from;
			To = to;
			IsTeleport = teleport;
		}
	}
}
