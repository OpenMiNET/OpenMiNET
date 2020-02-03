using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Ocean
{
    public class FrozenOceanBiome : BiomeBase
    {
        public FrozenOceanBiome()
        {
            Id = 10;
            Name = "Frozen Ocean";
            Temperature = 0.0f;
            Downfall = 0.5f;
            MinHeight = -1f;
            MaxHeight = 0.5f;
            Terrain = new OceanTerrain();
            
            Type = BiomeType.Ocean;
        }
    }
}