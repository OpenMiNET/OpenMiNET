using System;
using System.Collections.Generic;
using MiNET.Utils;
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
        
        
         public static float[] getChunkRTH(ChunkCoordinates chunk)
        {
            //CALCULATE RAIN
            var rainnoise = new FastNoise(123123);
            rainnoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
            rainnoise.SetFrequency(.007f); //.015
            rainnoise.SetFractalType(FastNoise.FractalType.FBM);
            rainnoise.SetFractalOctaves(1);
            rainnoise.SetFractalLacunarity(.25f);
            rainnoise.SetFractalGain(1);
            //CALCULATE TEMP
            var tempnoise = new FastNoise(123123 + 1);
            tempnoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
            tempnoise.SetFrequency(.004f); //.015f
            tempnoise.SetFractalType(FastNoise.FractalType.FBM);
            tempnoise.SetFractalOctaves(1);
            tempnoise.SetFractalLacunarity(.25f);
            tempnoise.SetFractalGain(1);
            
            float rain = rainnoise.GetNoise(chunk.X, chunk.Z) + 1;
            float temp = tempnoise.GetNoise(chunk.X, chunk.Z) + 1;
            float height = GetChunkHeightNoise(chunk.X, chunk.Z, 0.015f,2);;
            return new []{rain, temp, height};
        }

         private static readonly OpenSimplexNoise OpenNoise = new OpenSimplexNoise("a-seed".GetHashCode());


         /// <summary>
         /// 
         /// </summary>
         /// <param name="x"></param>
         /// <param name="z"></param>
         /// <param name="scale"></param>
         /// <param name="max"></param>
         /// <returns></returns>
         public static float GetChunkHeightNoise(int x, int z, float scale, int max)
         {//CALCULATE HEIGHT
             var heightnoise = new FastNoise(123123 + 2);
             heightnoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
             heightnoise.SetFrequency(scale);
             heightnoise.SetFractalType(FastNoise.FractalType.FBM);
             heightnoise.SetFractalOctaves(1);
             heightnoise.SetFractalLacunarity(2);
             heightnoise.SetFractalGain(.5f);
             return (heightnoise.GetNoise(x, z)+1 )*(max/2f);
             return (float)(OpenNoise.Evaluate(x * scale, z * scale) + 1f) * (max / 2f);
         }
           /// <summary>
           /// 
           /// </summary>
           /// <param name="x"></param>
           /// <param name="z"></param>
           /// <param name="max"></param>
           /// <returns></returns>
           public static float GetHeightNoiseBlock(int x, int z)
           {//CALCULATE HEIGHT
             
               var heightnoise = new FastNoise(123123 + 2);
               heightnoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
               heightnoise.SetFrequency(.015f/16);
               heightnoise.SetFractalType(FastNoise.FractalType.FBM);
               heightnoise.SetFractalOctaves(1);
               heightnoise.SetFractalLacunarity(2);
               heightnoise.SetFractalGain(.5f);
               return (heightnoise.GetNoise(x, z)+1 );
                              
               // return (float)(OpenNoise.Evaluate(x * scale, z * scale) + 1f) * (max / 2f);
           }public static float GetRainNoiseBlock(int x, int z)
           {//CALCULATE RAIN
               var rainnoise = new FastNoise(123123);
               rainnoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
               rainnoise.SetFrequency(.007f/16); //.015
               rainnoise.SetFractalType(FastNoise.FractalType.FBM);
               rainnoise.SetFractalOctaves(1);
               rainnoise.SetFractalLacunarity(.25f);
               rainnoise.SetFractalGain(1);
               return (rainnoise.GetNoise(x, z)+1 );
           }
           public static float GetTempNoiseBlock(int x, int z)
           {//CALCULATE TEMP
               var tempnoise = new FastNoise(123123 + 1);
               tempnoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
               tempnoise.SetFrequency(.004f/16); //.015f
               tempnoise.SetFractalType(FastNoise.FractalType.FBM);
               tempnoise.SetFractalOctaves(1);
               tempnoise.SetFractalLacunarity(.25f);
               tempnoise.SetFractalGain(1);
               return (tempnoise.GetNoise(x, z)+1 );
           }
         
        //CHECKED 5/10 @ 5:23 And this works fine!
        public static AdvancedBiome GetBiome(ChunkColumn chunk)
        {
            var rth = getChunkRTH(new ChunkCoordinates()
                    {
                        X = chunk.X,
                        Z = chunk.Z
                    });
            foreach (var biome in Biomes)
                if (biome.check(rth))
                {
                    bool BC = false;
                    for (int zz = -1; zz <= 1; zz++)
                    for (int xx = -1; xx <= 1; xx++)
                    {
                        if (xx == 0 && zz == 0) continue;
                        var tb = BiomeManager.GetBiome2(getChunkRTH(new ChunkCoordinates()
                        {
                            X = chunk.X+xx,
                            Z = chunk.Z+zz
                        }));
                        if (tb.LocalID != biome.LocalID)
                        {
                            BC = true;
                            break;
                        }
                    }
                    //CHEKC IF BOREDR CHUNK
                    //Top
                    
                    biome.BorderChunk = BC;
                    // Console.WriteLine($"BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB {BC}");
                    
                    // if (calculate > -1)
                    // {
                    //     
                    //     if(n.LocalID != -1)d[ab.LocalID]++;
                    //
                    //
                    //     var winner = -1;
                    //     var winner2 = -1;
                    //     var delta = 0;
                    //     var delta2 = 0;
                    //     var difc = 0;
                    //     for (var i = 0; i < d.Length; i++)
                    //     {
                    //         var c = d[i];
                    //         if (c > 0) difc++;
                    //         if (delta < c)
                    //         {
                    //             winner2 = winner;
                    //             delta2 = delta;
                    //             winner = i;
                    //             delta = c;
                    //         }
                    //         else if (delta2 < c)
                    //         {
                    //             winner2 = i;
                    //             delta2 = c;
                    //         }
                    //     }
                    //
                    //
                    //     var b = GetBiome(winner);
                    //     if (difc > 1)
                    //     {
                    //         b.BorderChunk = true;
                    //         b.BorderBiome = GetBiome(winner2);
                    //         if (winner2 == tb.LocalID)
                    //             b.BorderType = 1;
                    //         else if (winner2 == rb.LocalID)
                    //             b.BorderType = 2;
                    //         else if (winner2 == bb.LocalID)
                    //             b.BorderType = 3;
                    //         else if (winner2 == lb.LocalID)
                    //             b.BorderType = 4;
                    //     }
                    //     else
                    //     {
                    //         b.BorderType = 0;
                    //         b.BorderChunk = false;
                    //     }
                    //
                    //     return b;
                    // }

                    return biome;
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