using MiNET.Blocks;
using OpenAPI.WorldGenerator.Generators.Surfaces;
using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Desert
{
    public class DesertBiome : BiomeBase
    {
        public DesertBiome()
        {
            Id = 2;
            Name = "Desert";
            Temperature = 2.0f;
            Downfall = 0.0f;
            MinHeight = 0.1f;
            MaxHeight = 0.2f;
            SurfaceBlock = 12;
            SoilBlock = 24;
            
            
            Terrain = new DesertTerrain();
            Surface = new SurfaceBase(Config, new Sand(), new Sandstone());
        }
    }
}