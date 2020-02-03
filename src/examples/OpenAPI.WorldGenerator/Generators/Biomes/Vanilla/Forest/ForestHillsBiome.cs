using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Forest
{
    public class ForestHillsBiome : BiomeBase
    {
        public ForestHillsBiome()
        {
            Id = 18;
            Name = "Forest Hills";
            Temperature = 0.7f;
            Downfall = 0.8f;
            MinHeight = 0.2f;
            MaxHeight = 0.6f;
            Terrain = new ForestHillsTerrain();
        }
    }
}