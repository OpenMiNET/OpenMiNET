using OpenAPI.GameEngine.Games.Teams;
using OpenAPI.GameEngine.Models.Games.Maps;

namespace OpenAPI.GameEngine.Games.Configuration
{
    public class GameConfig
    {
        public string Name { get; set; } = "Unknown game";
        public bool AlwaysOpen { get; set; } = false;
        public TeamsConfiguration Teams { get; set; } = new TeamsConfiguration();

        public bool RequiresMap { get; set; } = true;
        public MapInfo[] Maps { get; set; } = new MapInfo[0];
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