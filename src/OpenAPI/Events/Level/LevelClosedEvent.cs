using OpenAPI.World;

namespace OpenAPI.Events.Level
{
	/// <summary>
	/// 	Dispatched when a <see cref="OpenLevel"/> gets closed.
	/// </summary>
	public class LevelClosedEvent : LevelEvent
	{
		/// <summary>
		/// 	
		/// </summary>
		/// <param name="world">The level that got closed</param>
		public LevelClosedEvent(OpenLevel world) : base(world)
		{
		}
	}
}
