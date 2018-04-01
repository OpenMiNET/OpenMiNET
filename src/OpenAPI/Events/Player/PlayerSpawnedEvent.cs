using OpenAPI.Player;

namespace OpenAPI.Events.Player
{
	public class PlayerSpawnedEvent : PlayerEvent
	{
		public PlayerSpawnedEvent(OpenPlayer player) : base(player)
		{
		}
	}
}
