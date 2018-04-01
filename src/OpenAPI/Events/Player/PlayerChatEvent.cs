using OpenAPI.Player;

namespace OpenAPI.Events.Player
{
	public class PlayerChatEvent : PlayerEvent
	{
		public string Message { get; }
		public PlayerChatEvent(OpenPlayer player, string message) : base(player)
		{
			Message = message;
		}
	}
}
