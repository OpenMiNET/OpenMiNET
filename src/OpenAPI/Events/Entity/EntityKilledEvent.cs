namespace OpenAPI.Events.Entity
{
	public class EntityKilledEvent : EntityEvent
	{
		public EntityKilledEvent(MiNET.Entities.Entity entity) : base(entity)
		{

		}
	}
}
