namespace OpenAPI.Events.Entity
{
	public class EntityEvent : Event
	{
		public MiNET.Entities.Entity Entity { get; }
		public EntityEvent(MiNET.Entities.Entity entity)
		{
			Entity = entity;
		}
	}
}
