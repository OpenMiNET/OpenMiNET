using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MiNET.Entities;
using MiNET.Utils;
using MiNET.Utils.Vectors;
using OpenAPI.Player;

namespace OpenAPI.Entities
{
    /// <summary>
    ///     Holds a disguised entity
    /// </summary>
    public class EntityDisguise
    {
        /// <summary>
        ///     The disguised entity
        /// </summary>
        public Entity Parent { get; }
        
        /// <summary>
        ///     The entity to disguise as
        /// </summary>
        public Entity Disguise { get; }
        
        /// <summary>
        ///     Should disguise be visible to <see cref="Parent"/>
        /// </summary>
        public bool SpawnToSelf { get; set; } = false;
        
        /// <summary>
        ///     The offset from the <see cref="Parent"/> entity.
        /// </summary>
        public Vector3 PositionOffset { get; set; } = Vector3.Zero;

        public EntityDisguise(Entity parent, Entity disguise)
        {
            Parent = parent;
            Disguise = disguise;

            Disguise.HealthManager = parent.HealthManager;
            Disguise.Level = Parent.Level;
            Disguise.KnownPosition = parent.KnownPosition;
            Disguise.NoAi = true;
        }

        public void Tick()
        {
            if (Disguise.Level != Parent.Level && Disguise.IsSpawned)
            {
                Disguise.DespawnEntity();
                Disguise.Level = Parent.Level;
                Disguise.SpawnEntity();
            }

           // if (Disguise.IsSpawned)
            {
                if (Disguise.KnownPosition != Parent.KnownPosition)
                {
                    Disguise.LastUpdatedTime = Parent.LastUpdatedTime;
                    Disguise.KnownPosition = new PlayerLocation(Parent.KnownPosition.X + PositionOffset.X, Parent.KnownPosition.Y + PositionOffset.Y,
                        Parent.KnownPosition.Z + PositionOffset.Z, Parent.KnownPosition.HeadYaw, Parent.KnownPosition.Yaw,
                        Parent.KnownPosition.Pitch);

                    Disguise.BroadcastMove(true);
                  //  if (Parent is OpenPlayer player)
                   // {
                   //     player.SendMessage("Disguise pos: " + Disguise.KnownPosition);
                    //}
                }
            }
        }

        public void SpawnDisguise()
        {
            Disguise.Level = Parent.Level;
            Disguise.SpawnEntity();
        }

        public void DespawnDisguise()
        {
            Disguise.DespawnEntity();
        }

        public void SpawnToPlayers(MiNET.Player[] players)
        {
            Disguise.Level = Parent.Level;

            if (!SpawnToSelf)
            {
                players = players.Where(x => x != Parent).ToArray();
            }

            Disguise.SpawnToPlayers(players);
        }

        public void DespawnFromPlayers(MiNET.Player[] players)
        {
            Disguise.DespawnFromPlayers(players);
        }
    }
}
