using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla
{
    public class SavannaPlateauBiome : BiomeBase
    {
        public SavannaPlateauBiome()
        {
            Id = 36;
            Name = "Savanna Plateau";
            Temperature = 1.0f;
            Downfall = 0.0f;
            MinHeight = 0.025f;
            MaxHeight = 1.5f;

            base.Config.AllowScenicLakes = false;

            Terrain = new SavannaPlateauTerrain(true, 35f, 160f, 60f, 40f, 69f);
        }
    }
}