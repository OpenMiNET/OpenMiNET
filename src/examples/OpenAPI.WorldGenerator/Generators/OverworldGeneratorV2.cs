using MiNET.Utils;
using MiNET.Worlds;

namespace OpenAPI.WorldGenerator.Generators
{
    public class OverworldGeneratorV2 : IWorldGenerator
    {
        public OverworldGeneratorV2()
        {
            int seed = "hello-world".GetHashCode();
            
        }
        
        public void Initialize()
        {
            
        }

        public ChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates)
        {
            throw new System.NotImplementedException();
        }
    }
}