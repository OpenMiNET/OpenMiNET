using MiNET.Utils;
using OpenAPI.Player;

namespace OpenAPI.Events.Player
{
	/// <summary>
	/// 	Dispatched whenever an <see cref="OpenPlayer"/> moves
	/// </summary>
	public class PlayerMoveEvent : PlayerEvent
	{
		/// <summary>
		/// 	The players old location
		/// </summary>
		public PlayerLocation From { get; }
		
		/// <summary>
		/// 	The players new location
		/// </summary>
		public PlayerLocation To { get; }
		
		/// <summary>
		/// 	Whether or not the player teleported
		/// </summary>
		public bool IsTeleport { get; }
		public PlayerMoveEvent(OpenPlayer player, PlayerLocation from, PlayerLocation to, bool teleport) : base(player)
		{
			From = from;
			To = to;
			IsTeleport = teleport;
		}
	}
}
