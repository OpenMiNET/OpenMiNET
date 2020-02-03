using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Taiga
{
    public class TaigaBiome : BiomeBase
    {
        public TaigaBiome()
        {
            Id = 5;
            Name = "Taiga";
            Temperature = 0.05f;
            Downfall = 0.8f;
            MinHeight = 0.1f;
            MaxHeight = 0.4f;
            Terrain = new TaigaTerrain();
        }
    }
}