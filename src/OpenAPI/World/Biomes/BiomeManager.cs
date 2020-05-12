using System.Collections.Generic;
using MiNET.Worlds;
using OpenAPI.World.Biomes;
using OpenAPI.World.Populator;

namespace OpenAPI.World
{
    public class BiomeManager
    {
        public static List<AdvancedBiome> Biomes = new List<AdvancedBiome>();

        private static int N;
        private static readonly Dictionary<int, AdvancedBiome> BiomeDict = new Dictionary<int, AdvancedBiome>();

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

        public static void AddBiome(AdvancedBiome biome)
        {
            biome.BorderChunk = false;
            Biomes.Add(biome);
            biome.LocalID = N;
            BiomeDict[N] = biome;
            N++;
        }

        public static AdvancedBiome GetBiome(int name)
        {
            foreach (var ab in Biomes)
                if (ab.LocalID == name)
                    return ab;

            return new SnowyIcyChunk();
        }

        public static AdvancedBiome GetBiome(string name)
        {
            foreach (var ab in Biomes)
                if (ab.name == name)
                    return ab;

            return new MainBiome();
        }

        //CHECKED 5/10 @ 5:23 And this works fine!
        public static AdvancedBiome GetBiome(float[] rth, ChunkColumn chunk, OpenExperimentalWorldProvider o,
                int calculate = 1)

            //ALERT
            //INFO
            //IMPORTANT
            //This value can be reduced... This is kina Heavy
        {
            foreach (var ab in Biomes)
                if (ab.check(rth))
                {
                    if (calculate > -1)
                    {
                        calculate--;
                        //Borer Biomes
                        var d = new int[Biomes.Count];
                        d.Fill(0, Biomes.Count);
                        AdvancedBiome n;
                        n = GetBiome(o.getChunkRTH(new ChunkColumn {X = chunk.X - 1, Z = chunk.Z + 1}), chunk, o,
                            calculate);
                        if(!n.BorderChunk && n.LocalID != -1)d[n.LocalID]++;
                        n = GetBiome(o.getChunkRTH(new ChunkColumn {X = chunk.X - 1, Z = chunk.Z - 1}), chunk, o,
                            calculate);
                        if(!n.BorderChunk && n.LocalID != -1)d[n.LocalID]++;
                        n = GetBiome(o.getChunkRTH(new ChunkColumn {X = chunk.X + 1, Z = chunk.Z + 1}), chunk, o,
                            calculate);
                        if(!n.BorderChunk && n.LocalID != -1)d[n.LocalID]++;
                        n = GetBiome(o.getChunkRTH(new ChunkColumn {X = chunk.X + 1, Z = chunk.Z - 1}), chunk, o,
                            calculate);
                        if(!n.BorderChunk && n.LocalID != -1)d[n.LocalID]++;

                        var tb = GetBiome(o.getChunkRTH(new ChunkColumn {X = chunk.X, Z = chunk.Z + 1}), chunk, o,
                            calculate);
                        if(!tb.BorderChunk && n.LocalID != -1) d[tb.LocalID]++;
                        var bb = GetBiome(o.getChunkRTH(new ChunkColumn {X = chunk.X, Z = chunk.Z - 1}), chunk, o,
                            calculate);
                        if(!bb.BorderChunk && n.LocalID != -1)d[bb.LocalID]++;

                        var rb = GetBiome(o.getChunkRTH(new ChunkColumn {X = chunk.X + 1, Z = chunk.Z}), chunk, o,
                            calculate);
                        if(!rb.BorderChunk && n.LocalID != -1)d[rb.LocalID]++;
                        var lb = GetBiome(o.getChunkRTH(new ChunkColumn {X = chunk.X - 1, Z = chunk.Z}), chunk, o,
                            calculate);
                        if(!lb.BorderChunk && n.LocalID != -1)d[lb.LocalID]++;
                        if(n.LocalID != -1)d[ab.LocalID]++;


                        var winner = -1;
                        var winner2 = -1;
                        var delta = 0;
                        var delta2 = 0;
                        var difc = 0;
                        for (var i = 0; i < d.Length; i++)
                        {
                            var c = d[i];
                            if (c > 0) difc++;
                            if (delta < c)
                            {
                                winner2 = winner;
                                delta2 = delta;
                                winner = i;
                                delta = c;
                            }
                            else if (delta2 < c)
                            {
                                winner2 = i;
                                delta2 = c;
                            }
                        }


                        var b = GetBiome(winner);
                        if (difc > 1)
                        {
                            b.BorderChunk = true;
                            b.BorderBiome = GetBiome(winner2);
                            if (winner2 == tb.LocalID)
                                b.BorderType = 1;
                            else if (winner2 == rb.LocalID)
                                b.BorderType = 2;
                            else if (winner2 == bb.LocalID)
                                b.BorderType = 3;
                            else if (winner2 == lb.LocalID)
                                b.BorderType = 4;
                        }
                        else
                        {
                            b.BorderType = 0;
                            b.BorderChunk = false;
                        }

                        return b;
                    }


                    return ab;
                }

            // return new MainBiome();
            return new WaterBiome();
            // return new HighPlains();
        }

        public static AdvancedBiome GetBiome2(float[] rth)
        {
            foreach (var ab in Biomes)
                if (ab.check(rth))
                    return ab;

            // return new MainBiome();
            return new WaterBiome();
            // return new HighPlains();
        }
    }
}