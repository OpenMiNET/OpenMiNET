using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Forest
{
    public class BirchForestHillsBiome : BiomeBase
    {
        public BirchForestHillsBiome()
        {
            Id = 28;
            Name = "Birch Forest Hills";
            Temperature = 0.6f;
            Downfall = 0.6f;
            MinHeight = 0.35f;
            MaxHeight = 0.45f;
            Terrain = new BirchForestHillsTerrain();
        }
    }
}