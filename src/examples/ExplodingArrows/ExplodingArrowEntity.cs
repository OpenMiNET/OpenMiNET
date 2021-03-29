using MiNET;
using MiNET.Blocks;
using MiNET.Entities;
using MiNET.Entities.Projectiles;
using MiNET.Utils;
using MiNET.Utils.Vectors;
using MiNET.Worlds;

namespace ExplodingArrows
{
    public class ExplodingArrowEntity : Arrow
    {
        public ExplodingArrowEntity(Player shooter, Level level, int damage = 2, bool isCritical = false) : base(shooter, level, damage, isCritical)
        {
            
        }

        protected override void OnHitBlock(Block blockCollided)
        {
            base.OnHitBlock(blockCollided);
            
            Explode(blockCollided.Coordinates);
        }

        protected override void OnHitEntity(Entity entityCollided)
        {
            base.OnHitEntity(entityCollided);
            
            Explode(entityCollided.KnownPosition.GetCoordinates3D());
        }

        private void Explode(BlockCoordinates location)
        {
            Explosion explosion = new Explosion(Level, location, 12, true);

            explosion.Explode();
        }
    }
}