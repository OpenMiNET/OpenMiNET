using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Mesa
{
    public class MesaPlateauBiome : BiomeBase
    {
        public MesaPlateauBiome()
        {
            Id = 39;
            Name = "Mesa Plateau";
            Temperature = 2.0f;
            Downfall = 0.0f;
            MinHeight = 1.5f;
            MaxHeight = 0.025f;

            SurfaceBlock = 12; //Surface = Red Sand
            SurfaceMetadata = 1;

            SoilBlock = 179; //Soil = Red Sandstone
            Terrain = new MesaPlateauTerrain(67);
        }
    }
}