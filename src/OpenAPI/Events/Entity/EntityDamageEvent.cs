using MiNET;

namespace OpenAPI.Events.Entity
{
	public class EntityDamageEvent : EntityEvent
	{
		public DamageCause Cause { get; }
		public int PreviousHealth { get; }
		public int NewHealth { get; }
		public MiNET.Entities.Entity Attacker { get; }

		public EntityDamageEvent(MiNET.Entities.Entity entity, MiNET.Entities.Entity source, DamageCause cause, int previousHealth, int newHealth) : base(entity)
		{
			Cause = cause;
			PreviousHealth = previousHealth;
			NewHealth = newHealth;
			Attacker = source;
		}
	}
}
