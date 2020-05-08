using System.Collections.Generic;

namespace OpenAPI.World
{
    public class BiomeManager
    {

        public BiomeManager()
        {
            // AddBiome(new MainBiome());
            AddBiome(new ForestBiome());
            AddBiome(new WarmForestBiome());
            AddBiome(new HotForestBiome());
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
        public static AdvancedBiome GetBiome(float rain,float temp)
        {
            foreach (var ab in Biomes)
            {
                if (ab.check(temp, rain)) return ab;
            }
            return new MainBiome();
        }
        
    }
}