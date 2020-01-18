using OpenAPI.WorldGenerator.Generators.Biomes;

namespace OpenAPI.WorldGenerator.Generators
{
    public class ChunkLandscape
    {
        public float[] Noise = new float[256];
        public BiomeBase[] Biome = new BiomeBase[256];
        public float[] River = new float[256];
    }
}