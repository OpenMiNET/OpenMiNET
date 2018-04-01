using OpenAPI.World;

namespace OpenAPI.Events.Level
{
	public class LevelInitEvent : LevelEvent
	{
		public LevelInitEvent(OpenLevel world) : base(world)
		{ 
		}
	}
}
