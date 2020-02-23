using OpenAPI.World;

namespace OpenAPI.Events.Level
{
	/// <summary>
	/// 	Dispatched when an <see cref="OpenLevel"/> gets initiated.
	/// </summary>
	public class LevelInitEvent : LevelEvent
	{
		/// <summary>
		/// 	
		/// </summary>
		/// <param name="world">The level that got initiated</param>
		public LevelInitEvent(OpenLevel world) : base(world)
		{ 
		}
	}
}
