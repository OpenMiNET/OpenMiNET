using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla
{
    public class PlainsBiome : BiomeBase
    {
        public PlainsBiome()
        {
            Id = 1;
            Name = "Plains";
            Temperature = 0.8f;
            Downfall = 0.4f;
            MinHeight = 0.0125f;
            MaxHeight = 0.5f;
            
            Terrain = new PlainsTerrain();
        }
    }
}