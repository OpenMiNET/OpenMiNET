using System;
using OpenAPI.Player;

namespace OpenAPI.Events.Player
{
	public class PlayerLoginCompleteEvent : PlayerEvent
	{
		public DateTime CompletionTime { get; }
		public PlayerLoginCompleteEvent(OpenPlayer player, DateTime loginCompleteTime) : base(player)
		{
			CompletionTime = loginCompleteTime;
		}
	}
}
