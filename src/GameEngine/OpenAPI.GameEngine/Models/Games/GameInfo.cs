using OpenAPI.GameEngine.Models.Games.Maps;

namespace OpenAPI.GameEngine.Models.Games
{
    public class GameInfo
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        
        public MapInfo[] Maps { get; set; }
    }
}