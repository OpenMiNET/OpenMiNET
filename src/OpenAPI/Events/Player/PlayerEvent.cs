using OpenAPI.Player;

namespace OpenAPI.Events.Player
{
	public class PlayerEvent : Event
	{
		public OpenPlayer Player { get; }
		public PlayerEvent(OpenPlayer player)
		{
			Player = player;
		}
	}
}
