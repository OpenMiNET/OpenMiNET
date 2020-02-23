using MiNET.Blocks;
using OpenAPI.WorldGenerator.Generators.Surfaces;
using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Desert
{
    public class DesertHillsBiome : BiomeBase
    {
        public DesertHillsBiome()
        {
            Id = 17;
            Name = "Desert Hills";
            Temperature = 2.0f;
            Downfall = 0.0f;
            MinHeight = 0.2f;
            MaxHeight = 0.7f;

            SurfaceBlock = 12; //Sand
            SoilBlock = 24; //Sandstone

            Terrain = new DesertHillsTerrain(10f, 80f, 68f, 200f);
            Surface = new SurfaceBase(Config, new Sand(), new Sandstone());
        }
    }
}