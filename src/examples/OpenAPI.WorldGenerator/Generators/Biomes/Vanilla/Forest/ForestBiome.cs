using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Forest
{
    public class ForestBiome : BiomeBase
    {
        public ForestBiome()
        {
            Id = 4;
            Name = "Forest";
            Temperature = 0.7f;
            Downfall = 0.8f;
            MinHeight = 0.1f; //TODO
            MaxHeight = 0.2f;
            Terrain = new ForestTerrain();
        }
    }
}