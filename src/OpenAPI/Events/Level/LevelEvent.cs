using OpenAPI.World;

namespace OpenAPI.Events.Level
{
	public class LevelEvent : Event
	{
		/// <summary>
		/// 	The level that the event occured in
		/// </summary>
		public OpenLevel Level { get; }
		public LevelEvent(OpenLevel world)
		{
			Level = world;
		}
	}
}
