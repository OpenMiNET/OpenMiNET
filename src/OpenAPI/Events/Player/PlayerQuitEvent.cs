using OpenAPI.Player;

namespace OpenAPI.Events.Player
{
	public class PlayerQuitEvent : PlayerEvent
	{
		public PlayerQuitEvent(OpenPlayer player) : base(player)
		{
		}
	}
}
