using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Ocean
{
    public class DeepOceanBiome : BiomeBase
    {
        public DeepOceanBiome()
        {
            Id = 24;
            Name = "Deep Ocean";
            Temperature = 0.5f;
            Downfall = 0.5f;
            MinHeight = -1.8F;
            MaxHeight = 0.1f;
            Terrain = new DeepOceanTerrain();
            
            
            Type = BiomeType.Ocean;
        }
    }
}