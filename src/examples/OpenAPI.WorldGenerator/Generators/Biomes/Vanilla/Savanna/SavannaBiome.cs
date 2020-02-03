using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla
{
    public class SavannaBiome : BiomeBase
    {
        public SavannaBiome()
        {
            Id = 35;
            Name = "Savanna";
            Temperature = 1.2f;
            Downfall = 0.0f;
            MinHeight = 0.005f;
            MaxHeight = 0.125f;
            
            Terrain = new SavannaTerrain();
        }
    }
}