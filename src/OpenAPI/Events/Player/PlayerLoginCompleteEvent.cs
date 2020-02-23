using System;
using OpenAPI.Player;

namespace OpenAPI.Events.Player
{
	/// <summary>
	/// 	Dispatched when an <see cref="OpenPlayer"/> completes the login cycle
	/// </summary>
	public class PlayerLoginCompleteEvent : PlayerEvent
	{
		/// <summary>
		/// 	The time the player completed the login
		/// </summary>
		public DateTime CompletionTime { get; }
		public PlayerLoginCompleteEvent(OpenPlayer player, DateTime loginCompleteTime) : base(player)
		{
			CompletionTime = loginCompleteTime;
		}
	}
}
