using MiNET.Blocks;
using OpenAPI.WorldGenerator.Generators.Surfaces;
using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Beach
{
    public class StoneBeachBiome : BiomeBase
    {
        public StoneBeachBiome()
        {
            Id = 25;
            Name = "Stone Beach";
            Temperature = 0.2f;
            Downfall = 0.3f;
            MinHeight = 0.1f;
            MaxHeight = 0.8f;

            Terrain = new BeachTerrain();
            Surface = new SurfaceBase(Config, new Gravel(), new Sandstone())
            {
                
            };
            
            Type = BiomeType.Beach | BiomeType.Cold | BiomeType.Land;
        }
    }
}