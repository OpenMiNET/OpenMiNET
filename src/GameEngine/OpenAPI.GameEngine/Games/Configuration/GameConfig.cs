using OpenAPI.GameEngine.Games.Teams;

namespace OpenAPI.GameEngine.Games.Configuration
{
    public class GameConfig
    {
        public string Name { get; set; } = "Unknown game";

        public TeamsConfiguration Teams { get; set; } = new TeamsConfiguration();
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