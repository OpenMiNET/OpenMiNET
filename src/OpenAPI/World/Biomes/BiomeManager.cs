using System.Collections.Generic;
using OpenAPI.World.Biomes;
using OpenAPI.World.Populator;

namespace OpenAPI.World
{
    public class BiomeManager
    {

        public BiomeManager()
        {
            // AddBiome(new MainBiome());
            AddBiome(new ForestBiome());
            AddBiome(new SnowyIcyChunk());
            AddBiome(new Desert());
            AddBiome(new Mountains());
            AddBiome(new Plains());
            AddBiome(new HighPlains());
            AddBiome(new WaterBiome());
            AddBiome(new ForestBiome());
            AddBiome(new SnowForest());
            AddBiome(new SnowTundra());
            AddBiome(new SnowyIcyChunk());
            AddBiome(new TropicalRainForest());
            AddBiome(new TropicalSeasonalForest());
        }

        public static List<AdvancedBiome> Biomes = new List<AdvancedBiome>();

        public static void AddBiome(AdvancedBiome biome)
        {
            Biomes.Add(biome);
        }

        public static AdvancedBiome GetBiome(string name)
        {
            foreach (var ab in Biomes)
            {
                if (ab.name == name) return ab;
            }
            return new MainBiome();
        }
        public static AdvancedBiome GetBiome(float[] rth)
        {
            foreach (AdvancedBiome ab in Biomes)
            {
                if (ab.check(rth)) return ab;
            }
            // return new MainBiome();
            return new WaterBiome();
            // return new HighPlains();

        }
        
    }
}