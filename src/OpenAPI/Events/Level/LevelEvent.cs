using OpenAPI.World;

namespace OpenAPI.Events.Level
{
	public class LevelEvent : Event
	{
		public OpenLevel Level { get; }
		public LevelEvent(OpenLevel world)
		{
			Level = world;
		}
	}
}
