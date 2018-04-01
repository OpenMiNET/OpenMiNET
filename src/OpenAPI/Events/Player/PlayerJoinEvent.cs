using OpenAPI.Player;

namespace OpenAPI.Events.Player
{
	public class PlayerJoinEvent : PlayerEvent
	{
		public PlayerJoinEvent(OpenPlayer player) : base(player)
		{
		}
	}
}
