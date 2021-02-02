using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;
using OpenAPI.GameEngine.Games.Configuration;

namespace OpenAPI.GameEngine.Models.Games.Maps
{
    public class MapInfo
    {
        public string    Name    { get; set; }
        public WorldType Type    { get; set; } = WorldType.Anvil;
        public string    Path    { get; set; }
        public bool      Enabled { get; set; }

        public TeamsConfiguration                                 Teams      { get; set; } = null;
        public ReadOnlyDictionary<string, GameStageConfiguration> Stages     { get; set; } = null;
        public JObject                                            Properties { get; set; } = new JObject();
    }
}