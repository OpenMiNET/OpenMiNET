using System;
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
            Console.WriteLine(biome.name+" >>>>> IS "+biome.BorderChunk);
            biome.BorderChunk = false;
            Biomes.Add(biome);
            biome.LocalID = N;
            BiomeDict[N] = biome;
            N++;
        }

        public static AdvancedBiome GetBiome(int name)
        {
            foreach (var ab in Biomes)
            {
                if (ab.LocalID == name)
                {
                    
                    Console.WriteLine(ab.name+" >>>>> IS GET FROM INT "+ab.BorderChunk);
                    return ab;
                }
            }

            return new SnowyIcyChunk();
        }

        public static AdvancedBiome GetBiome(string name)
        {
            foreach (var ab in Biomes)
            {
                if (ab.name == name)
                {
                    
                    Console.WriteLine(ab.name+" >>>>> IS GET FROM NAME "+ab.BorderChunk);
                    return ab;
                }
            }

            return new MainBiome();
        }

        //CHECKED 5/10 @ 5:23 And this works fine!
        public static AdvancedBiome GetBiome(float[] rth, ChunkColumn chunk, OpenExperimentalWorldProvider o,
                int calculate = 0)

            //ALERT
            //INFO
            //IMPORTANT
            //This value can be reduced... This is kina Heavy
        {
            foreach (AdvancedBiome ab in Biomes)
            {
                if (ab.check(rth))
                {
                    if (calculate > -1)
                    {
                        calculate--;
                        //Borer Biomes
                        int[] d = new int[BiomeManager.Biomes.Count];
                        d.Fill(0, Biomes.Count);
                        AdvancedBiome n;
                        n = GetBiome(o.getChunkRTH(new ChunkColumn() {X = chunk.X - 1, Z = chunk.Z + 1}), chunk, o,
                            calculate);
                        d[n.LocalID]++;
                        n = GetBiome(o.getChunkRTH(new ChunkColumn() {X = chunk.X - 1, Z = chunk.Z - 1}), chunk, o,
                            calculate);
                        d[n.LocalID]++;
                        n = GetBiome(o.getChunkRTH(new ChunkColumn() {X = chunk.X + 1, Z = chunk.Z + 1}), chunk, o,
                            calculate);
                        d[n.LocalID]++;
                        n = GetBiome(o.getChunkRTH(new ChunkColumn() {X = chunk.X + 1, Z = chunk.Z - 1}), chunk, o,
                            calculate);
                        d[n.LocalID]++;

                        n = GetBiome(o.getChunkRTH(new ChunkColumn() {X = chunk.X, Z = chunk.Z + 1}), chunk, o,
                            calculate);
                        d[n.LocalID]++;
                        n = GetBiome(o.getChunkRTH(new ChunkColumn() {X = chunk.X, Z = chunk.Z - 1}), chunk, o,
                            calculate);
                        d[n.LocalID]++;

                        n = GetBiome(o.getChunkRTH(new ChunkColumn() {X = chunk.X + 1, Z = chunk.Z}), chunk, o,
                            calculate);
                        d[n.LocalID]++;
                        n = GetBiome(o.getChunkRTH(new ChunkColumn() {X = chunk.X - 1, Z = chunk.Z}), chunk, o,
                            calculate);
                        d[n.LocalID]++;
                        d[ab.LocalID]++;
                       
                        

                        int winner = -1;
                        int winner2 = -1;
                        int delta = 0;
                        int delta2 = 0;
                        int difc = 0;
                        for (int i = 0; i < d.Length; i++)
                        {
                            int c = d[i];
                            if (c > 0) difc++;
                            if (delta < c)
                            {
                                winner2 = winner;
                                delta2 = delta;
                                winner = i;
                                delta = c;
                            }else if (delta2 < c)
                            {
                                winner2 = i;
                                delta2 = c;
                            }
                        }
                        
                        // Console.WriteLine($"++++++>>>> WINNDER 1: {winner} ======= {delta}");
                        // Console.WriteLine($"++++++>>>> WINNDER 2: {winner2} ======= {delta2}");
                        // Console.WriteLine($"++++++>>>> difc 2: {difc} ");


                        var b = GetBiome(winner);
                        Console.WriteLine(b.name+" AAAAAA >>>>> IS  FROM ADV "+b.BorderChunk+" BUT DIFC + "+difc);
                        if (difc > 1)
                        {
                            // int ddd = 0;
                            // foreach (var dd in d)
                            // {
                            //     Console.WriteLine($"++++++>>>> {ddd} ======= {dd}");
                            //     ddd++;
                            // }
                            // Console.WriteLine($"++++++>>>>=-===========================");
                            // Console.WriteLine($"DIFFFFFFFFFFFFFFFFFFFFFFF BETWEENB {winner} &*& {winner2}");
                            b.BorderChunk = true;
                            b.BorderBiome = GetBiome(winner2);
                        }
                        else b.BorderChunk = false;

                        Console.WriteLine(ab.name+" >>>>> IS GET FROM ADV "+ab.BorderChunk);
                        Console.WriteLine(b.name+" BBBBB>>>>> IS GET FROM ADV "+b.BorderChunk);
                        return b;
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