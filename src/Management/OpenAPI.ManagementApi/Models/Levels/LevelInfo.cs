using System.Collections.Generic;

namespace OpenAPI.ManagementApi.Models.Levels
{
    public class LevelInfo
    {
        public string Id { get; set; }
        public int LoadedChunks { get; set; }
        public int PlayerCount { get; set; }
        
        public dynamic Players { get; set; }
    }
}