using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.ExtremeHills
{
    public class ExtremeHillsBiome : BiomeBase
    {
        public ExtremeHillsBiome()
        {
            Id = 3;
            Name = "Extreme Hills";
            Temperature = 0.2f;
            Downfall = 0.3f;
            MinHeight = 0.2f;
            MaxHeight = 1f;
            Terrain = new RidgedExtremeHillsTerrain(150f, 67f, 200f);
        }
    }
}