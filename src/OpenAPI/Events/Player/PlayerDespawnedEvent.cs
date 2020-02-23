using OpenAPI.Player;

namespace OpenAPI.Events.Player
{
	/// <summary>
	/// 	Dispatched whenever an <see cref="OpenPlayer"/> despawns
	/// </summary>
	public class PlayerDespawnedEvent : PlayerEvent
	{
		public PlayerDespawnedEvent(OpenPlayer player) : base(player)
		{
		}
	}
}
