using MiNET.Utils;
using OpenAPI.GameEngine.Games.Configuration;
using MapInfo = OpenAPI.GameEngine.Models.Games.Maps.MapInfo;

namespace TNTRun
{
    public class TNTRunConfig : GameConfig<TNTRunMapConfig>
    {
            
    }
    
    public class TNTRunMapConfig : MapInfo
    {
        public PlayerLocation LobbySpawn { get; set; } = new PlayerLocation();
        public PlayerLocation GameSpawn { get; set; } = new PlayerLocation();
    }
}