using OpenAPI.Player;

namespace OpenAPI.Events.Player
{
	/// <summary>
	/// 	Dispatched when an <see cref="OpenPlayer"/> joins the server
	/// </summary>
	public class PlayerJoinEvent : PlayerEvent
	{
		public PlayerJoinEvent(OpenPlayer player) : base(player)
		{
		}
	}
}
