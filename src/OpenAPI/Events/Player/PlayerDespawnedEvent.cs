using OpenAPI.Player;

namespace OpenAPI.Events.Player
{
	public class PlayerDespawnedEvent : PlayerEvent
	{
		public PlayerDespawnedEvent(OpenPlayer player) : base(player)
		{
		}
	}
}
