using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MiNET.Blocks;
using MiNET.Net;
using MiNET.Utils;
using MiNET.Worlds;

namespace OpenAPI.World
{
    public class BiomeQualifications
    {
        public int baseheight = 70;

        public int heightvariation;
        public float startheight; //0-2
        public float startrain; //0 - 1
        public float starttemp; //0 - 2
        public float stopheight;
        public float stoprain;
        public float stoptemp;

        // public int baseheight = 20;
        public bool waterbiome;


        public BiomeQualifications(float startrain, float stoprain, float starttemp, float stoptemp, float startheight,
            float stopheight, int heightvariation, bool waterbiome = false)
        {
            this.startrain = startrain;
            this.stoprain = stoprain;
            this.starttemp = starttemp;
            this.stoptemp = stoptemp;
            this.startheight = startheight;
            this.stopheight = stopheight;
            this.waterbiome = waterbiome;
            this.heightvariation = heightvariation;
        }


        public bool check(float[] rth)
        {
            var rain = rth[0];
            var temp = rth[1];
            var height = rth[2];
            return startrain <= rain && stoprain >= rain && starttemp <= temp && stoptemp >= temp &&
                   startheight <= height && stopheight >= height;
        }

        public bool check(float rain, float temp, float height)
        {
            return startrain <= rain && stoprain >= rain && starttemp <= temp && stoptemp >= temp &&
                   startheight <= height && stopheight >= height;
        }
    }

    public abstract class AdvancedBiome
    {
        private static readonly OpenSimplexNoise OpenNoise = new OpenSimplexNoise("a-seed".GetHashCode());
        public BiomeQualifications BiomeQualifications;

        /// <summary>
        /// </summary>
        public AdvancedBiome BorderBiome;

        /// <summary>
        /// </summary>
        public bool BorderChunk = false;

        public FastNoise HeightNoise = new FastNoise(121212);

        public int LocalID = -1;
        public string name;
        public Random RNDM = new Random();
        public int startheight = 80;

        public AdvancedBiome(string name, BiomeQualifications bq)
        {
            BiomeQualifications = bq;
            HeightNoise.SetGradientPerturbAmp(3);
            HeightNoise.SetFrequency(.24f);
            HeightNoise.SetNoiseType(FastNoise.NoiseType.CubicFractal);
            HeightNoise.SetFractalOctaves(2);
            HeightNoise.SetFractalLacunarity(.35f);
            HeightNoise.SetFractalGain(1);
            this.name = name;
        }

        public int BorderType { get; set; } = 0;

        public bool check(float[] rth)
        {
            return BiomeQualifications.check(rth);
        }

        public async Task<ChunkColumn> preSmooth(OpenExperimentalWorldProvider openExperimentalWorldProvider,
            ChunkColumn chunk,
            float[] rth)
        {
            var t = new Stopwatch();
            t.Start();
            SmoothChunk(openExperimentalWorldProvider, chunk, rth);
            t.Stop();

            return chunk;
            // Console.WriteLine($"CHUNK SMOOTHING OF {chunk.X} {chunk.Z} TOOK {t.Elapsed}");
        }

        public async Task<ChunkColumn> prePopulate(OpenExperimentalWorldProvider openExperimentalWorldProvider,
            ChunkColumn chunk,
            float[] rth)
        {
            var t = new Stopwatch();
            t.Start();
            // OpenServer.FastThreadPool.QueueUserWorkItem(() => { PopulateChunk(openExperimentalWorldProvider,chunk, rth); });
            //CHECK IF BORDER CHUNK AND CHANGE SETTINGS
            // if (BorderChunk)
            // {
            //     var bro = BorderBiome;
            //     // Console.WriteLine($"OLD VALUE WAS {bro.BiomeQualifications.heightvariation} New Val is \\/\\/\\/\\/ || "+BiomeQualifications.heightvariation);
            //     var h = (bro.BiomeQualifications.heightvariation + BiomeQualifications.heightvariation) / 2;
            //     // Console.WriteLine("+++++++++++++++++++++++++++++++++++++ "+h);
            //     BiomeQualifications.heightvariation = h;
            //     // Console.WriteLine($"THE CHUNK AT {chunk.X} {chunk.Z} IS A BORDER CHUNK WITH VAL {bro} |||");
            // }

            PopulateChunk(openExperimentalWorldProvider, chunk, rth);

            t.Stop();
            // int minWorker, minIOC,maxworker,maxIOC;
            // ThreadPool.GetMinThreads(out minWorker, out minIOC);
            // ThreadPool.GetMaxThreads(out maxworker, out maxIOC);
            // if(minWorker != 20  && !ThreadPool.SetMinThreads(20,20))Console.WriteLine("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            var a = OpenServer.FastThreadPool.Settings.NumThreads;
            // SmoothChunk(openExperimentalWorldProvider,chunk,rth);
            // Console.WriteLine($"CHUNK POPULATION OF {chunk.X} {chunk.Z} TOOK {t.Elapsed} ||| {a}");
            return chunk;
        }

        /// <summary>
        ///     Populate Chunk from Biome
        /// </summary>
        /// <param name="openExperimentalWorldProvider"></param>
        /// <param name="c"></param>
        /// <param name="rth"></param>
        public abstract /*Task*/ void PopulateChunk(OpenExperimentalWorldProvider openExperimentalWorldProvider,
            ChunkColumn c,
            float[] rth);

        public int[,] CreateMapFrom2Chunks(ChunkColumn c1, ChunkColumn c2, int pos)
        {
            if (pos == 0 || pos > 4) return null;

            var map = new int[0, 0];
            if (pos == 1)
                //TOP
                map = new int[16, 32];
            else if (pos == 2)
                //RIGHT
                map = new int[32, 16];
            else if (pos == 3)
                //Bottom
                map = new int[16, 32];
            else if (pos == 4)
                //LEFT
                map = new int[32, 16];

            for (var x = 0; x < map.GetLength(0); x++)
            for (var z = 0; z < map.GetLength(1); z++)
            {
                if (pos == 1)
                {
                    if (x < 16 && z < 16)
                        map[x, z] = c1.GetHeight(x, z);
                    else
                        map[x, z] = c2.GetHeight(x, z - 16);
                }
                else if (pos == 2)
                {
                    if (x < 16 && z < 16)
                        map[x, z] = c1.GetHeight(x, z);
                    else
                        map[x, z] = c2.GetHeight(x - 16, z);
                }
                else if (pos == 3)
                {
                    if (x < 16 && z < 16)
                        map[x, z] = c2.GetHeight(x, z);
                    else
                        map[x, z] = c1.GetHeight(x, z - 16);
                }
                else if (pos == 4)
                {
                    if (x < 16 && z < 16)
                        map[x, z] = c2.GetHeight(x, z);
                    else
                        map[x, z] = c1.GetHeight(x - 16, z);
                }

                // if (x == 0 || z == 0 || z == 16 || x == 16)
                // {
                //     map[x, z] = 157;
                // }
            }

            return map;
        }

        public int getNeighbor(int x, int z, int xo, int zo, int[,] map)
        {
            if (x + xo >= map.GetLength(0) || x + xo < 0) return -1;
            if (z + zo >= map.GetLength(1) || z + zo < 0) return -1;
            // Console.WriteLine($"{x+xo} || {z+zo}");
            return map[x + xo, z + zo];
        }

        List<String> Ran = new List<string>();

        public void SmoothToNeighbor(int x, int z, int xo, int zo, int max, int[,] map)
        {
            var ch = map[x, z];
            //Top
            if (Ran.Contains(x + xo + "||" + z + zo) || x == 0 || z == 0) return;
            var n = getNeighbor(x, z, xo, zo, map);
            if (n != -1)
            {
                // int d = Math.Abs(ch - n);

                // var d = new Random().Next(0, 3);
                if (n > ch)
                {
                    int vvv = Math.Abs(n - ch);


                    if (vvv < 2) vvv = 2;
                    int vv = new Random().Next(1, vvv);
                    // int v = Math.Max(2, vv);
                    var d = new Random().Next(0, vv);
                    /*if (d > 3)*/
                    d = new Random().Next(0, 2);
                    n = ch + d;
                }
                else
                {
                    int vvv = Math.Abs(n - ch) / 2;
                    if (vvv < 2) vvv = 2;
                    int vv = new Random().Next(1, vvv);
                    // int v = Math.Max(2, vv);
                    var d = new Random().Next(0, vv);
                    /*if (d > 3)*/
                    d = new Random().Next(0, 2);
                    n = ch - d;
                }

                map[x + xo, z + zo] = n;
            }
        }

        public int[,] SmoothMapV2(int[,] map)
        {
            int[,] newmap = new int[map.GetLength(0), map.GetLength(1)];
            // int[,]  newmap = map;
            for (int x = 0; x < map.GetLength(0); x++)
            {
                int th = map[x, map.GetLength(1) - 1];
                for (int z = 0; z < map.GetLength(1); z++)
                {
                    if (x == 0 || x == 16 || z == 0 || z == 16)
                    {
                        newmap[x, z] = map[x, z];
                        continue;
                    }

                    int lh = map[x, z - 1];
                    int ch = map[x, z];
                    int dist = map.GetLength(0) - x;
                    int hd = th - ch;
                    int delta;
                    if (hd != 0)
                    {
                        delta = dist / hd;
                    }
                    else
                    {
                        delta = 0;
                    }

                    if (delta > 0)
                    {
                        //SUBTRACT
                        // int nch = (int) Math.Floor((ch + th) / 2f);
                        int nch = ch - 1;
                        if (nch >= lh) nch = lh - 1;
                        newmap[x, z] = nch;
                    }
                    else if (delta == 0)
                    {
                        newmap[x, z] = ch;
                    }
                    else
                    {
                        int nch = ch - 1;
                        if (nch >= lh) nch = lh - 1;
                        newmap[x, z] = nch;
                        //ADD
                    }
                }
            }

            return newmap;
        }

        public static void printDisplayTable(int[,] table, string title = "Testing1")
        {
            var mx = table.GetLength(0);
            var mz = table.GetLength(1);

            var t2 = 58 - title.Length / 2;
            if (title.Length % 2 != 0) t2++;
            var t1 = 58 - title.Length / 2;
            var c1 = new string[t1];
            var c2 = new string[t2];
            c1.Fill("=", t1);
            c2.Fill("=", t2);
            var mastertitle = c1._tostring() + title + c2._tostring();
            Console.WriteLine(mastertitle);
            for (var z = 0; z < mz; z++)
            {
                var s = "";
                for (var x = 0; x < mx; x++)
                {
                    s += $"||{table[x, z]}";
                    if (x + 1 == mx) s += "||";
                }

                Console.WriteLine(s);
            }

            Console.WriteLine("==========================================================");
        }


        public void FormatChunk(int x, int z, int xo, int zo, int dif, ChunkColumn chunk, ChunkColumn nc)
        {
            if (x < 16 && z < 16)
            {
                for (var y = BiomeQualifications.baseheight; y < 255; y++)
                {
                    if (y > dif)
                    {
                        if (chunk.GetBlockId(x, y, z) == 0) break;
                        chunk.SetBlock(x, y, z, new Air());
                    }else if (y == dif)
                    {

                        chunk.SetBlock(x, y, z, new EmeraldBlock());
                    }
                    else
                    {
                        if (chunk.GetBlockId(x, y, z) == 0)
                        {
                            chunk.SetBlock(x, y, z, new Stone());
                        }
                    }
                }
            }
            else
            {
                for (var y = BiomeQualifications.baseheight; y < 255; y++)
                {
                    if (y > dif)
                    {
                        if (nc.GetBlockId(x - xo, y, z - zo) == 0) break;
                        nc.SetBlock(x - xo, y, z - zo, new Air());
                    }else if (y == dif)
                    {

                        nc.SetBlock(x-xo, y, z-zo, new EmeraldBlock());
                    }
                    else
                    {
                        if (nc.GetBlockId(x - xo, y, z - zo) == 0)
                        {
                            nc.SetBlock(x - xo, y, z - zo, new Stone());
                        }
                    }
                }
            }
        }

        public void SmoothChunk(OpenExperimentalWorldProvider o, ChunkColumn chunk, float[] rth)
        {
            //Smooth Biome

            if (BorderChunk)
            {
                chunk.SetBlock(8, 110, 8, new EmeraldBlock());
                var workingchunk = BorderBiome;
                AdvancedBiome n;
                var nc = new ChunkColumn();
                var pos = 0;
                int[,] h = null;
                var i = -1;

                if (BorderType != 0)
                {
                    Console.WriteLine("YESSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSS" + BorderType);
                    pos = BorderType;
                    Console.WriteLine("YESSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSS" + pos);
                    if (pos == 1)
                        nc = o.GenerateChunkColumn(new ChunkCoordinates {X = chunk.X, Z = chunk.Z + 1}, false, false);
                    else if (pos == 2)
                        nc = o.GenerateChunkColumn(new ChunkCoordinates {X = chunk.X + 1, Z = chunk.Z}, false, false);

                    else if (pos == 3)
                        nc = o.GenerateChunkColumn(new ChunkCoordinates {X = chunk.X, Z = chunk.Z - 1}, false, false);

                    else if (pos == 4)
                        nc = o.GenerateChunkColumn(new ChunkCoordinates {X = chunk.X - 1, Z = chunk.Z}, false, false);
                    if (nc == null)
                    {
                        Console.WriteLine(
                            $"WHOOOOOOAAAAAAAAAAAAA >>>>>>>>>>>>> THASDASDASD111111111222321 IS NULL {pos}");
                    }

                    h = CreateMapFrom2Chunks(chunk, nc, pos);
                }

                // if (pos == 0)
                // {
                //     Console.WriteLine("OLD WAY RUNNING!");
                //     n = BiomeManager.GetBiome(o.getChunkRTH(new ChunkColumn {X = chunk.X, Z = chunk.Z + 1}), chunk, o,
                //         i);
                //     if (n.LocalID == workingchunk.LocalID && pos == 0)
                //     {
                //         //TOP
                //         nc = o.GenerateChunkColumn(new ChunkCoordinates {X = chunk.X, Z = chunk.Z + 1}, true);
                //         if (nc != null)
                //         {
                //             pos = 1;
                //             h = CreateMapFrom2Chunks(chunk,
                //                 nc, pos);
                //         }
                //     }
                //
                //     n = BiomeManager.GetBiome(o.getChunkRTH(new ChunkColumn {X = chunk.X + 1, Z = chunk.Z}), chunk, o,
                //         i);
                //     if (n.LocalID == workingchunk.LocalID && pos == 0)
                //     {
                //         //RIGHT
                //         nc = o.GenerateChunkColumn(new ChunkCoordinates {X = chunk.X + 1, Z = chunk.Z}, true);
                //         if (nc != null)
                //         {
                //             pos = 2;
                //             h = CreateMapFrom2Chunks(chunk, nc, pos);
                //         }
                //     }
                //
                //     n = BiomeManager.GetBiome(o.getChunkRTH(new ChunkColumn {X = chunk.X, Z = chunk.Z - 1}), chunk, o,
                //         i);
                //     if (n.LocalID == workingchunk.LocalID && pos == 0)
                //     {
                //         //BOTTOM
                //         nc = o.GenerateChunkColumn(new ChunkCoordinates {X = chunk.X, Z = chunk.Z - 1}, true);
                //         if (nc != null)
                //         {
                //             pos = 3;
                //             h = CreateMapFrom2Chunks(chunk, nc, pos);
                //         }
                //     }
                //
                //     n = BiomeManager.GetBiome(o.getChunkRTH(new ChunkColumn {X = chunk.X - 1, Z = chunk.Z}), chunk, o,
                //         i);
                //     if (n.LocalID == workingchunk.LocalID && pos == 0)
                //     {
                //         //LEFT
                //         nc = o.GenerateChunkColumn(new ChunkCoordinates {X = chunk.X - 1, Z = chunk.Z}, true);
                //         if (nc != null)
                //         {
                //             pos = 4;
                //             h = CreateMapFrom2Chunks(chunk,
                //                 nc, pos);
                //         }
                //     }
                // }

                if (pos == 0)
                {
                    Console.WriteLine("ERRRRRRRRRRRR NOOOOOOOOOOaaaaaaaa SMOOOOOOOOOOOOOOTHHHHHHHHH");
                }
                else
                {
                    // var nh = SmoothMapV2(h);
                    // printDisplayTable(nh);

                    // chunk.SetBlock(8, 109, 8, new Netherrack());
                    // chunk.SetBlock(8, 109 - pos, 8, new EmeraldBlock());
                    Console.WriteLine($"ABOUT TO SMOOTH BUT POS={pos} NC={nc} ");
                }

                if (pos != 0 && nc != null)
                {
                    chunk.SetBlock(8, 111, 8, new RedstoneBlock());
                    Console.WriteLine($"SMOOTHING CHUNK {chunk.X} {chunk.Z}");
                    var nh = SmoothMapV2(h);

                    // printDisplayTable(nh);
                    var sx = chunk.X >> 16;
                    var sz = chunk.Z >> 16;
                    if (pos == 3 && pos == 4)
                    {
                        sx = nc.X >> 16;
                        sz = nc.Z >> 16;
                    }

                    var stopx = nc.X >> (16 + 15);
                    var stopz = nc.Z >> (16 + 15);

                    // var xx = 0;
                    // var zz = 0;
                    Console.WriteLine($" X:{nh.GetLength(0)} ||| Z:{nh.GetLength(1)}");
                    for (var z = 0; z < nh.GetLength(1); z++)
                    {
                        for (var x = 0; x < nh.GetLength(0); x++)
                        {
                            // Console.WriteLine($"STARTING ON {x} ::: {z}");
                            // for (var z = sz; z < stopz; z++)
                            // {
                            //     for (var x = sx; x < stopx; x++)
                            //     {
                            var dif = nh[x, z];
                            // dif = h[x, z];

                            // for (var y = 50; y < 255; y++)
                            // {
                            //     if (y >= dif)
                            //         o.Level.SetBlock(new Air()
                            //         {
                            //             Coordinates = new BlockCoordinates(sx+x,y,sz+z)
                            //         });
                            //     if (o.Level.GetBlock(new PlayerLocation(x, y, z)).Id == 0) break;
                            // }

                            if (pos == 1 || pos == 3)
                            {
                                FormatChunk(x, z, 0, 16, dif, chunk, nc);
                                if (pos == 1) chunk.SetBlock(8, 110, 8 + 1, new Furnace());
                                if (pos == 3) chunk.SetBlock(8, 110, 8 - 1, new Furnace());
                            }
                            else if (pos == 2 || pos == 4)
                            {
                                FormatChunk(x, z, 16, 0, dif, chunk, nc);
                                // if(x == 0 || x == 16 )
                                if (pos == 2) chunk.SetBlock(8+1, 110, 8 , new Furnace());
                                if (pos == 4) chunk.SetBlock(8+1, 110, 8 , new Furnace());
                            }

                            // xx++;
                        }

                        // xx = 0;
                        // zz++;
                    }
                }

                Console.WriteLine("=========================||======================");
            }
        }

        public static AdvancedBiome GetBiome(int biomeId)
        {
            return BiomeManager.GetBiome(biomeId);
        }


        public static float GetNoise(int x, int z, float scale, int max)
        {
            var heightnoise = new FastNoise(123123 + 2);
            heightnoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
            heightnoise.SetFrequency(scale);
            heightnoise.SetFractalType(FastNoise.FractalType.FBM);
            heightnoise.SetFractalOctaves(1);
            heightnoise.SetFractalLacunarity(2);
            heightnoise.SetFractalGain(.5f);
            return (heightnoise.GetNoise(x, z)+1 )*(max/2f);
            // return (float) ((OpenNoise.Evaluate(x * scale, z * scale) + 1f) * (max / 2f));
        }
    }
}