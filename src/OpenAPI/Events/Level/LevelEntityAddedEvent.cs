using OpenAPI.World;

namespace OpenAPI.Events.Level
{
	public class LevelEntityAddedEvent : LevelEvent
	{
		public MiNET.Entities.Entity Entity { get; }
		public LevelEntityAddedEvent(OpenLevel world, MiNET.Entities.Entity entity) : base(world)
		{
			Entity = entity;
		}
	}
}
