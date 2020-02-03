using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Beach;

namespace OpenAPI.WorldGenerator.Generators.Biomes
{
    public enum BeachType
    {
        Cold,
        Stone,
        Normal
    }

    public static class BeachExtensions
    {
        public static BiomeBase GetBiome(this BeachType type)
        {
            if (type == BeachType.Stone)
                return new StoneBeachBiome();

            if (type == BeachType.Cold)
                return new ColdBeachBiome();
            
            return new BeachBiome();
        }
    }
}