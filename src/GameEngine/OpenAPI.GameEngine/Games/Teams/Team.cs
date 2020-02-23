using System.Collections.Concurrent;
using System.Threading.Tasks;
using OpenAPI.GameEngine.Games.Configuration;
using OpenAPI.Player;

namespace OpenAPI.GameEngine.Games.Teams
{
    public class Team
    {
        public string Name { get; set; }
        public int PlayerCount => Members.Count;
        
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

        public virtual bool TryJoin(OpenPlayer player)
        {
            lock (_teamLock)
            {
                if (PlayerCount < MaxPlayers)
                {
                    return Members.TryAdd(player.GetIdentifier(), player);
                }
            }

            return false;
        }

        public virtual void Leave(OpenPlayer player)
        {
            Members.TryRemove(player.GetIdentifier(), out _);
        }
    }
}