using System.Collections.Generic;
using MiNET.Worlds;
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

        private static int N = 0;
        static Dictionary<int, AdvancedBiome> BiomeDict = new Dictionary<int, AdvancedBiome>();

        public static void AddBiome(AdvancedBiome biome)
        {
            Biomes.Add(biome);
            biome.LocalID = N;
            BiomeDict[N] = biome;
            N++;
        }

        public static AdvancedBiome GetBiome(int name)
        {
            foreach (var ab in Biomes)
            {
                if (ab.LocalID == name) return ab;
            }

            return new SnowyIcyChunk();
        }
        public static AdvancedBiome GetBiome(string name)
        {
            foreach (var ab in Biomes)
            {
                if (ab.name == name) return ab;
            }

            return new MainBiome();
        }

        public static AdvancedBiome GetBiome(float[] rth, ChunkColumn chunk, OpenExperimentalWorldProvider o,
            int calculate = 2)
        {
            foreach (AdvancedBiome ab in Biomes)
            {
                if (ab.check(rth))
                {
                    if (calculate == -1)
                    {
                        calculate--;
                        //Borer Biomes
                        int[] d = new int[BiomeManager.Biomes.Count];
                        d.Fill(0, Biomes.Count);
                        AdvancedBiome n;
                        n = GetBiome(o.getChunkRTH(new ChunkColumn() {X = chunk.X - 1, Z = chunk.Z + 1}), chunk, o,calculate);
                        d[n.LocalID]++;
                        n = GetBiome(o.getChunkRTH(new ChunkColumn() {X = chunk.X - 1, Z = chunk.Z - 1}), chunk, o,calculate);
                        d[n.LocalID]++;
                        n = GetBiome(o.getChunkRTH(new ChunkColumn() {X = chunk.X + 1, Z = chunk.Z + 1}), chunk, o,calculate);
                        d[n.LocalID]++;
                        n = GetBiome(o.getChunkRTH(new ChunkColumn() {X = chunk.X + 1, Z = chunk.Z - 1}), chunk, o,calculate);
                        d[n.LocalID]++;

                        n = GetBiome(o.getChunkRTH(new ChunkColumn() {X = chunk.X, Z = chunk.Z + 1}), chunk, o,calculate);
                        d[n.LocalID]++;
                        n = GetBiome(o.getChunkRTH(new ChunkColumn() {X = chunk.X, Z = chunk.Z - 1}), chunk, o,calculate);
                        d[n.LocalID]++;

                        n = GetBiome(o.getChunkRTH(new ChunkColumn() {X = chunk.X + 1, Z = chunk.Z}), chunk, o,calculate);
                        d[n.LocalID]++;
                        n = GetBiome(o.getChunkRTH(new ChunkColumn() {X = chunk.X - 1, Z = chunk.Z}), chunk, o,calculate);
                        d[n.LocalID]++;

                        int winner = -1;
                        int delta = 0;
                        for (int i = 0; i < d.Length; i++)
                        {
                            int c = d[i];
                            if (delta < c)
                            {
                                winner = i;
                                delta = c;
                            }
                        }

                        return GetBiome(winner);
                    }


                    return ab;
                }
            }

            // return new MainBiome();
            return new WaterBiome();
            // return new HighPlains();
        }
        
         public static AdvancedBiome GetBiome2(float[] rth)
        {
            foreach (AdvancedBiome ab in Biomes)
            {
                if (ab.check(rth))
                {
                    return ab;
                }
            }

            // return new MainBiome();
            return new WaterBiome();
            // return new HighPlains();
        }
    }
}