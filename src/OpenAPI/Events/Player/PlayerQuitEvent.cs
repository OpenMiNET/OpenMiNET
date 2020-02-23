using OpenAPI.Player;

namespace OpenAPI.Events.Player
{
	/// <summary>
	/// 	Dispatched when an <see cref="OpenPlayer"/> leaves the server
	/// </summary>
	public class PlayerQuitEvent : PlayerEvent
	{
		public PlayerQuitEvent(OpenPlayer player) : base(player)
		{
		}
	}
}
