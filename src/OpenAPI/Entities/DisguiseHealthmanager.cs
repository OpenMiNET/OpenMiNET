using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiNET;
using MiNET.Entities;
using MiNET.Items;

namespace OpenAPI.Entities
{
    public class DisguiseHealthManager : OpenHealthManager
    {
        private Entity Parent { get; }
        public DisguiseHealthManager(Entity entity, Entity parent) : base(entity)
        {
            Parent = parent;
        }

        protected override void DoKnockback(Entity source, Item tool)
        {
            base.DoKnockback(source, tool);
        }

        public override void Kill()
        {
            base.Kill();
        }

        public override void Regen(int amount = 1)
        {
            base.Regen(amount);
        }

        public override void TakeHit(Entity source, Item tool, int damage = 1, DamageCause cause = DamageCause.Unknown)
        {
            base.TakeHit(source, tool, damage, cause);
        }

        public override void TakeHit(Entity source, int damage = 1, DamageCause cause = DamageCause.Unknown)
        {
            base.TakeHit(source, damage, cause);
        }
    }
}
