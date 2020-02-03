using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Jungle
{
    public class JungleBiome : BiomeBase
    {
        public JungleBiome()
        {
            Id = 21;
            Name = "Jungle";
            Temperature = 1.2f;
            Downfall = 0.9f;
            MinHeight = 0.1f;
            MaxHeight = 0.4f;

            Terrain = new JungleTerrain();
        }
    }
}