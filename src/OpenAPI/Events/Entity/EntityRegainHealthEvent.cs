namespace OpenAPI.Events.Entity
{
	public class EntityRegainHealthEvent : EntityEvent
	{
		public int PreviousHealth { get; }
		public int NewHealth { get; }
		public EntityRegainHealthEvent(MiNET.Entities.Entity entity, int previousHealth, int newHealth) : base(entity)
		{
			PreviousHealth = previousHealth;
			NewHealth = newHealth;
		}
	}
}
