using System.Linq;
using Newtonsoft.Json;
using OpenAPI.GameEngine.Games.Teams;
using OpenAPI.GameEngine.Models.Games.Maps;

namespace OpenAPI.GameEngine.Games.Configuration
{
    public class GameConfig<TMap> where TMap : MapInfo
    {
        public string Name { get; set; } = "Unknown game";
        public TeamsConfiguration Teams { get; set; } = new TeamsConfiguration();

        public bool RequiresMap { get; set; } = true;
        
        public TMap[] Maps { get; set; } = new TMap[0];
    }

    public class TeamsConfiguration
    {
        public int Min { get; set; } = 2;
        public int Max { get; set; } = 8;

        public int MinPlayers { get; set; } = 1;
        public int MaxPlayers { get; set; } = 1;

        public TeamFillMode FillMode { get; set; } = TeamFillMode.FillMinSpread;
    }
}