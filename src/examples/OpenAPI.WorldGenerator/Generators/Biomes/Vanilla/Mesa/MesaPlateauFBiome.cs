using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Mesa
{
    public class MesaPlateauFBiome : BiomeBase
    {
        public MesaPlateauFBiome()
        {
            Id = 38;
            Name = "Mesa Plateau F";
            Temperature = 2.0f;
            Downfall = 0.0f;
            MinHeight = 1.5f;
            MaxHeight = 0.25f;

            SurfaceBlock = 12; //Surface = Red Sand
            SurfaceMetadata = 1;

            SoilBlock = 179; //Soil = Red Sandstone
            Terrain = new MesaPlateauTerrain(67);
        }
    }
}