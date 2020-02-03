namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla
{
    public class RiverBiome : BiomeBase
    {
        public RiverBiome()
        {
            Id = 7;
            Name = "River";
            Temperature = 0.5f;
            Downfall = 0.5f;
            MinHeight = -0.5f;
            MaxHeight = 0f;
        }
    }
}