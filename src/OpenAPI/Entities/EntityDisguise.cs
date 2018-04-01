using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MiNET.Entities;
using MiNET.Utils;
using OpenAPI.Player;

namespace OpenAPI.Entities
{
    public class EntityDisguise
    {
        public Entity Parent { get; }
        public Entity Disguise { get; }
        public bool SpawnToSelf { get; set; } = false;
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

        public void SpawnDesguise()
        {
            Disguise.Level = Parent.Level;
            Disguise.SpawnEntity();
        }

        public void DespawnDesguise()
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
