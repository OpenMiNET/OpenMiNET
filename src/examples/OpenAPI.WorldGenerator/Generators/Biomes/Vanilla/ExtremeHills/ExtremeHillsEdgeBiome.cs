using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.ExtremeHills
{
    public class ExtremeHillsEdgeBiome : BiomeBase
    {
        public ExtremeHillsEdgeBiome()
        {
            Id = 20;
            Name = "Extreme Hills Edge";
            Temperature = 0.2f;
            Downfall = 0.3f;
            MinHeight = 0.2f;
            MaxHeight = 0.8f;
            Terrain = new RidgedExtremeHillsTerrain(125f, 67f, 200f);
        }
    }
}