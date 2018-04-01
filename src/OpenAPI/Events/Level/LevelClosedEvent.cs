using OpenAPI.World;

namespace OpenAPI.Events.Level
{
	public class LevelClosedEvent : LevelEvent
	{
		public LevelClosedEvent(OpenLevel world) : base(world)
		{
		}
	}
}
