using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MiNET.Utils;
using OpenAPI.GameEngine.Games.Configuration;
using OpenAPI.Player;

namespace OpenAPI.GameEngine.Games.Teams
{
    public class Team
    {
        public  string Name { get; set; }
        public  int PlayerCount => Members.Count;
        public  int AlivePlayers => Members.Count(x => !x.Value.HealthManager.IsDead);
        
        private int MaxPlayers { get; set; }
        private ConcurrentDictionary<string, OpenPlayer> Members { get; }
        private object _teamLock = new object();
        
        public Team(string name, int maxPlayers)
        {
            Name = name;
            MaxPlayers = maxPlayers;
            Members = new ConcurrentDictionary<string, OpenPlayer>();
        }

        public Team()
        {
            
        }

        public IEnumerable<OpenPlayer> Players
        {
            get
            {
                lock (_teamLock)
                {
                    return Members.Values.ToArray();
                }
            }
        }

        public virtual bool TryJoin(OpenPlayer player)
        {
            lock (_teamLock)
            {
                if (PlayerCount < MaxPlayers)
                {
                    if (Members.TryAdd(player.GetIdentifier(), player))
                    {
                        player.SetTeam(this);
                        return true;
                    }
                }
            }

            return false;
        }

        public virtual void Leave(OpenPlayer player)
        {
            if (Members.TryRemove(player.GetIdentifier(), out _))
            {
                player.SetTeam(null);
            }
        }

        public void ForEachMember(Action<OpenPlayer> action)
        {
            foreach (var member in Members.Values)
            {
                action?.Invoke(member);
            }
        }

        public void TeleportTo(PlayerLocation location)
        {
            ForEachMember(
                p =>
                {
                    p.SpawnPosition = location;
                    p.Teleport(location);
                });
        }
    }
}