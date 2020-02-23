using OpenAPI.Player;

namespace OpenAPI.Events.Player
{
	/// <summary>
	/// 	Dispatched when an <see cref="OpenPlayer"/> spawns in a world
	/// </summary>
	public class PlayerSpawnedEvent : PlayerEvent
	{
		public PlayerSpawnedEvent(OpenPlayer player) : base(player)
		{
		}
	}
}
