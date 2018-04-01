using MiNET;
using MiNET.Entities;
using MiNET.Items;
using OpenAPI.Events.Entity;
using OpenAPI.World;

namespace OpenAPI.Entities
{
	public class OpenHealthManager : HealthManager
	{
		public OpenHealthManager(Entity entity) : base(entity)
		{
			
		}
		
		public override void Regen(int amount = 1)
		{
			if (Entity.Level is OpenLevel lvl)
			{
				int h = Health;
				EntityRegainHealthEvent killedEvent = new EntityRegainHealthEvent(Entity, h, h + amount);
				lvl.EventDispatcher.DispatchEvent(killedEvent);
				if (killedEvent.IsCancelled)
				{
					return;
				}
			}

			base.Regen(amount);
		}

		private object _killSync = new object();

		public override void Kill()
		{
			if (Entity.Level is OpenLevel lvl)
			{
				EntityKilledEvent killedEvent = new EntityKilledEvent(Entity);
				lvl.EventDispatcher.DispatchEvent(killedEvent);
				if (killedEvent.IsCancelled)
				{
					return;
				}
			}

		    if (Entity is MiNET.Player player)
		    {
		        ResetHealth();

		        player.SendUpdateAttributes();
                player.BroadcastSetEntityData();

                if (!player.KeepInventory)
		        {
		            player.DropInventory();
		        }

                player.SetPosition(player.SpawnPosition);
            }
		    else
		    {
		        lock (_killSync)
		        {
		            if (IsDead) return;
		            IsDead = true;

		            Health = 0;
		        }

                Entity.BroadcastEntityEvent();
		        Entity.BroadcastSetEntityData();
		        Entity.DespawnEntity();

		        if (LastDamageSource is MiNET.Player)
		        {
		            var drops = Entity.GetDrops();
		            foreach (var drop in drops)
		            {
		                Entity.Level.DropItem(Entity.KnownPosition.ToVector3(), drop);
		            }
		        }
            }

			/*if (Entity is MiNET.Player player)
			{
				player.SendUpdateAttributes();
				player.BroadcastEntityEvent();

				player.BroadcastSetEntityData();
				player.DespawnEntity();

				if (!player.KeepInventory)
				{
					player.DropInventory();
				}

				var mcpeRespawn = McpeRespawn.CreateObject();
				mcpeRespawn.x = player.SpawnPosition.X;
				mcpeRespawn.y = player.SpawnPosition.Y;
				mcpeRespawn.z = player.SpawnPosition.Z;
				player.SendPackage(mcpeRespawn);
			}
			else
			{
				Entity.BroadcastEntityEvent();
				Entity.BroadcastSetEntityData();
				Entity.DespawnEntity();

				if (LastDamageSource is MiNET.Player)
				{
					var drops = Entity.GetDrops();
					foreach (var drop in drops)
					{
						Entity.Level.DropItem(Entity.KnownPosition.ToVector3(), drop);
					}
				}
			}*/
		}

		public override void TakeHit(Entity source, Item tool, int damage = 1, DamageCause cause = DamageCause.Unknown)
		{
			if (Entity.Level is OpenLevel lvl)
			{
				int h = Health;
				EntityDamageEvent damageEvent = new EntityDamageEvent(base.Entity, source,
					cause, h, h - damage);
				lvl.EventDispatcher.DispatchEvent(damageEvent);
				if (damageEvent.IsCancelled)
				{
					return;
				}
			}

			base.TakeHit(source, tool, damage, cause);
		}

		public override void TakeHit(Entity source, int damage = 1, DamageCause cause = DamageCause.Unknown)
		{
			if (Entity.Level is OpenLevel lvl)
			{
				int h = Health;
				EntityDamageEvent damageEvent = new EntityDamageEvent(base.Entity, source,
					cause, h, h - damage);
				lvl.EventDispatcher.DispatchEvent(damageEvent);
				if (damageEvent.IsCancelled)
				{
					return;
				}
			}

			base.TakeHit(source, damage, cause);
		}
	}
}
