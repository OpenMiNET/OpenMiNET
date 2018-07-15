using OpenAPI.Player;

namespace OpenAPI.Events.Player
{
	public class PlayerChatEvent : PlayerEvent
	{
		private string _original;
		public string Message { get; set; }
		public PlayerChatEvent(OpenPlayer player, string message) : base(player)
		{
			_original = message;
			Message = message;
		}
	}
}
