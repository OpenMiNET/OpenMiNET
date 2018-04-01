using OpenAPI.World;

namespace OpenAPI.Events.Level
{
	public class LevelEntityRemovedEvent : LevelEvent
	{
		public MiNET.Entities.Entity Entity { get; }
		public LevelEntityRemovedEvent(OpenLevel world, MiNET.Entities.Entity entity) : base(world)
		{
			Entity = entity;
		}
	}
}
