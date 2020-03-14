using MiNET;
using MiNET.Items;
using MiNET.Utils;
using MiNET.Worlds;

namespace ExplodingArrows.Items
{
    public class ExplodingArrowsBow : ItemBow
    {
        public override void Release(Level world, Player player, BlockCoordinates blockCoordinates, long timeUsed)
        {
            var inventory = player.Inventory;
            bool haveArrows = player.GameMode == GameMode.Creative;
            haveArrows = haveArrows || this.GetEnchantingLevel(EnchantingType.Infinity) > 0;
            if (!haveArrows)
            {
                for (byte i = 0; i < inventory.Slots.Count; i++)
                {
                    var itemStack = inventory.Slots[i];
                    if (itemStack.Id == 262)
                    {
                        if (--itemStack.Count <= 0)
                        {
                            // set empty
                            inventory.Slots[i] = new ItemAir();
                        }
                        haveArrows = true;
                        break;
                    }
                }
            }
            if (!haveArrows) return;
            if (timeUsed < 6) return; // questionable, but we go with it for now.

            float force = CalculateForce(timeUsed);
            if (force < 0.1D) return;

            var arrow = new ExplodingArrowEntity(player, world, 2, !(force < 1.0));
            arrow.PowerLevel = this.GetEnchantingLevel(EnchantingType.Power);
            arrow.KnownPosition = (PlayerLocation) player.KnownPosition.Clone();
            arrow.KnownPosition.Y += 1.62f;

            arrow.Velocity = arrow.KnownPosition.GetHeadDirection().Normalize() * (force * 3);
            arrow.KnownPosition.Yaw = (float) arrow.Velocity.GetYaw();
            arrow.KnownPosition.Pitch = (float) arrow.Velocity.GetPitch();
            arrow.BroadcastMovement = true;
            arrow.DespawnOnImpact = false;

            arrow.SpawnEntity();
            player.Inventory.DamageItemInHand(ItemDamageReason.ItemUse, player, null);
        }
        
        private float CalculateForce(long timeUsed)
        {
            float force = timeUsed / 20.0F;

            force = ((force * force) + (force * 2.0F)) / 3.0F;
            if (force < 0.1D)
            {
                return 0;
            }

            if (force > 1.0F)
            {
                force = 1.0F;
            }

            return force;
        }
    }
}