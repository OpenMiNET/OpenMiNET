using OpenAPI.Player;

namespace OpenAPI.Events.Player
{
	/// <summary>
	/// 	The base class for any <see cref="OpenPlayer"/> event
	/// </summary>
	public class PlayerEvent : Event
	{
		/// <summary>
		/// 	The player that the event occured for.
		/// </summary>
		public OpenPlayer Player { get; }
		
		
		public PlayerEvent(OpenPlayer player)
		{
			Player = player;
		}
	}
}
