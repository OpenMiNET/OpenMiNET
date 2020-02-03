using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Jungle
{
    public class JungleEdgeBiome : BiomeBase
    {
        public JungleEdgeBiome()
        {
            Id = 23;
            Name = "Jungle Edge";
            Temperature = 0.95f;
            Downfall = 0.8f;
            MinHeight = 0.1f;
            MaxHeight = 0.2f;

            Terrain = new JungleEdgeTerrain();
        }
    }
}