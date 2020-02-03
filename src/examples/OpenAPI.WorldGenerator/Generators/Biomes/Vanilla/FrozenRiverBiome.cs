namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla
{
    public class FrozenRiverBiome : BiomeBase
    {
        public FrozenRiverBiome()
        {
            Id = 11;
            Name = "Frozen River";
            Temperature = 0.0f;
            Downfall = 0.5f;
            MinHeight = -0.5f;
            MaxHeight = 0f;

            Type = BiomeType.River | BiomeType.Cold | BiomeType.Snowy;
        }
    }
}