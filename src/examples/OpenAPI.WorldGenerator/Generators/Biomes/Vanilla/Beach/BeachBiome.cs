using MiNET.Blocks;
using OpenAPI.WorldGenerator.Generators.Surfaces;
using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Beach
{
    public class BeachBiome : BiomeBase
    {
        public BeachBiome()
        {
            Id = 16;
            Name = "Beach";
            Temperature = 0.8f;
            Downfall = 0.4f;
            MinHeight = 0f;
            MaxHeight = 0.1f;
            Terrain = new BeachTerrain();
            Surface = new SurfaceBase(Config, new Sand(), new Sandstone())
            {
                
            };
            
            Type = BiomeType.Beach | BiomeType.Land;
        }
    }
}