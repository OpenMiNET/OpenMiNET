using MiNET.Blocks;
using OpenAPI.WorldGenerator.Generators.Surfaces;
using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Beach
{
    public class ColdBeachBiome : BiomeBase
    {
        public ColdBeachBiome()
        {
            Id = 26;
            Name = "Cold Beach";
            Temperature = 0.05f;
            Downfall = 0.3f;
            MinHeight = 0f;
            MaxHeight = 0.025f;

            Terrain = new BeachTerrain();
            Surface = new SurfaceBase(Config, new Sand(), new Sandstone())
            {
                
            };
            
            Type = BiomeType.Beach | BiomeType.Cold | BiomeType.Land;
        }   
    }
}