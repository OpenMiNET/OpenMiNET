using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Jungle
{
    public class JungleHillsBiome : BiomeBase
    {
        public JungleHillsBiome()
        {
            Id = 22;
            Name = "Jungle Hills";
            Temperature = 1.2f;
            Downfall = 0.9f;
            MinHeight = 0.2f;
            MaxHeight = 1.8f;

            Terrain = new JungleHillsTerrain(72f, 40f);
        }
    }
}