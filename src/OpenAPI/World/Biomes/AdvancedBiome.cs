using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using LibNoise.Writer;
using MiNET;
using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Worlds;

namespace OpenAPI.World
{
    public class BiomeQualifications
    {
        public float startrain; //0 - 1
        public float stoprain;
        public float starttemp; //0 - 2
        public float stoptemp;
        public float startheight; //0-2
        public float stopheight;

        public int heightvariation;

        public int baseheight = 70;

        // public int baseheight = 20;
        public bool waterbiome = false;


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
            float rain = rth[0];
            float temp = rth[1];
            float height = rth[2];
            return (startrain <= rain && stoprain >= rain && starttemp <= temp && stoptemp >= temp &&
                    startheight <= height && stopheight >= height);
        }

        public bool check(float rain, float temp, float height)
        {
            return (startrain <= rain && stoprain >= rain && starttemp <= temp && stoptemp >= temp &&
                    startheight <= height && stopheight >= height);
        }
    }

    public abstract class AdvancedBiome
    {
        /// <summary>
        /// 
        /// </summary>
        public bool BorderChunk = false;

        /// <summary>
        /// 
        /// </summary>
        public AdvancedBiome BorderBiome;

        public int LocalID = -1;
        public String name;
        public Random RNDM = new Random();
        public int startheight = 80;
        public BiomeQualifications BiomeQualifications;
        public FastNoise HeightNoise = new FastNoise(121212);

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

        public bool check(float[] rth)
        {
            return BiomeQualifications.check(rth);
        }

        public void preSmooth(OpenExperimentalWorldProvider openExperimentalWorldProvider, ChunkColumn chunk,
            float[] rth)
        {
            var t = new Stopwatch();
            t.Start();
            SmoothChunk(openExperimentalWorldProvider, chunk, rth);
            t.Stop();
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
            if (BorderChunk)
            {
                var bro = BorderBiome;
                // Console.WriteLine($"OLD VALUE WAS {bro.BiomeQualifications.heightvariation} New Val is \\/\\/\\/\\/ || "+BiomeQualifications.heightvariation);
                int h = (bro.BiomeQualifications.heightvariation + BiomeQualifications.heightvariation) / 2;
                // Console.WriteLine("+++++++++++++++++++++++++++++++++++++ "+h);
                BiomeQualifications.heightvariation = h;
                // Console.WriteLine($"THE CHUNK AT {chunk.X} {chunk.Z} IS A BORDER CHUNK WITH VAL {bro} |||");
            }

            PopulateChunk(openExperimentalWorldProvider, chunk, rth);
            if (BorderChunk)
            {
                chunk.SetBlock(8, 110, 8, new EmeraldBlock());
            }

            t.Stop();
            // int minWorker, minIOC,maxworker,maxIOC;
            // ThreadPool.GetMinThreads(out minWorker, out minIOC);
            // ThreadPool.GetMaxThreads(out maxworker, out maxIOC);
            // if(minWorker != 20  && !ThreadPool.SetMinThreads(20,20))Console.WriteLine("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            var a = OpenServer.FastThreadPool.Settings.NumThreads;
            // Console.WriteLine($"CHUNK POPULATION OF {chunk.X} {chunk.Z} TOOK {t.Elapsed} ||| {a}");
            return chunk;
        }

        /// <summary>
        /// Populate Chunk from Biome
        /// </summary>
        /// <param name="openExperimentalWorldProvider"></param>
        /// <param name="c"></param>
        /// <param name="rth"></param>
        public abstract /*Task*/ void PopulateChunk(OpenExperimentalWorldProvider openExperimentalWorldProvider,
            ChunkColumn c,
            float[] rth);

        public int[,] CreateMapFrom2Chunks(ChunkColumn c1, ChunkColumn c2, int pos)
        {
            if (pos == 0 || pos > 4)
            {
                return null;
            }

            int[,] map = new int[0, 0];
            if (pos == 1)
            {
                //TOP
                map = new int[16, 32];
            }
            else if (pos == 2)
            {
                //RIGHT
                map = new int[32, 16];
            }
            else if (pos == 3)
            {
                //Bottom
                map = new int[16, 32];
            }
            else if (pos == 4)
            {
                //LEFT
                map = new int[32, 16];
            }

            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int z = 0; z < map.GetLength(1); z++)
                {
                    if (pos == 1)
                    {
                        if (x < 16 && z < 16)
                        {
                            map[x, z] = c2.GetHeight(x, z);
                        }
                        else
                        {
                            map[x, z] = c1.GetHeight(x, z - 16);
                        }
                    }
                    else if (pos == 2)
                    {
                        if (x < 16 && z < 16)
                        {
                            map[x, z] = c1.GetHeight(x, z);
                        }
                        else
                        {
                            map[x, z] = c2.GetHeight(x - 16, z);
                        }
                    }
                    else if (pos == 3)
                    {
                        if (x < 16 && z < 16)
                        {
                            map[x, z] = c1.GetHeight(x, z);
                        }
                        else
                        {
                            map[x, z] = c2.GetHeight(x, z - 16);
                        }
                    }
                    else if (pos == 4)
                    {
                        if (x < 16 && z < 16)
                        {
                            map[x, z] = c2.GetHeight(x, z);
                        }
                        else
                        {
                            map[x, z] = c1.GetHeight(x - 16, z);
                        }
                    }
                }
            }

            return map;
        }

        public int getNeighbor(int x, int z, int xo, int zo, int max, int[,] map)
        {
            if (x + xo >= max || x + xo < 0) return -1;
            if (z + zo >= max || z + zo < 0) return -1;
            // Console.WriteLine($"{x+xo} || {z+zo}");
            return map[x + xo, z + zo];
        }

        public void SmoothToNeighbor(int x, int z, int xo, int zo, int max, int[,] map)
        {
            int ch = map[x, z];
            //Top
            var n = getNeighbor(x, z, xo, zo, max, map);
            if (n != -1)
            {
                // int d = Math.Abs(ch - n);
                int d = new Random().Next(0, 3);
                if (n > ch)
                {
                    n = ch + d;
                }
                else
                {
                    n = ch - d;
                }

                map[x + xo, z + zo] = n;
            }
        }

        public int[,] SmoothMapV2(int[,] map)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int z = 0; z < map.GetLength(1); z++)
                {
                    //TopLeft
                    SmoothToNeighbor(x, z, -1, 1, map.GetLength(1), map);
                    //TopMiddle
                    SmoothToNeighbor(x, z, 0, 1, map.GetLength(1), map);
                    //TopRight
                    SmoothToNeighbor(x, z, 1, 1, map.GetLength(1), map);

                    //MIDLE


                    //MidLeft
                    SmoothToNeighbor(x, z, -1, 0, map.GetLength(1), map);
                    //MidRight
                    SmoothToNeighbor(x, z, 1, 0, map.GetLength(1), map);

                    //BOTTOM
                    //BottomLeft
                    SmoothToNeighbor(x, z, -1, -1, map.GetLength(1), map);
                    //BottomMiddle
                    SmoothToNeighbor(x, z, 0, -1, map.GetLength(1), map);
                    //BottomRight
                    SmoothToNeighbor(x, z, 1, -1, map.GetLength(1), map);
                }
            }

            return map;
        }

        public static void printDisplayTable(int[,] table, string title = "Testing1")
        {
            int mx = table.GetLength(0);
            int mz = table.GetLength(1);

            int t2 = 58 - title.Length / 2;
            if (title.Length % 2 != 0) t2++;
            int t1 = 58 - title.Length / 2;
            string[] c1 = new string[t1];
            string[] c2 = new string[t2];
            c1.Fill("=", t1);
            c2.Fill("=", t2);
            String mastertitle = c1._tostring() + title + c2._tostring();
            Console.WriteLine(mastertitle);
            for (int z = 0; z < mz; z++)
            {
                String s = "";
                for (int x = 0; x < mx; x++)
                {
                    s += $"||{table[x, z]}";
                    if (x + 1 == mx) s += "||";
                }

                Console.WriteLine(s);
            }

            Console.WriteLine($"==========================================================");
        }


        public void SmoothChunk(OpenExperimentalWorldProvider o, ChunkColumn chunk, float[] rth)
        {
            //Smooth Biome

            if (BorderChunk)
            {
                var workingchunk = BorderBiome;
                AdvancedBiome n;
                ChunkColumn nc ;
                int pos = 0;
                int[,] h = null;
                n = BiomeManager.GetBiome(o.getChunkRTH(new ChunkColumn() {X = chunk.X, Z = chunk.Z + 1}), chunk, o);
                if (n.LocalID == workingchunk.LocalID && pos == 0)
                {
                    //TOP
                    nc = o.GenerateChunkColumn(new ChunkCoordinates() {X = chunk.X, Z = chunk.Z + 1}, true);
                    if (nc != null)
                    {
                        pos = 1;
                        h = CreateMapFrom2Chunks(chunk,
                            nc, pos);
                    }
                }

                n = BiomeManager.GetBiome(o.getChunkRTH(new ChunkColumn() {X = chunk.X + 1, Z = chunk.Z}), chunk, o);
                if (n.LocalID == workingchunk.LocalID && pos == 0)
                {
                    //RIGHT
                    nc = o.GenerateChunkColumn(new ChunkCoordinates() {X = chunk.X + 1, Z = chunk.Z}, true);
                    if (nc != null)
                    {
                        pos = 2;
                        h = CreateMapFrom2Chunks(chunk, nc, pos);
                    }
                }

                n = BiomeManager.GetBiome(o.getChunkRTH(new ChunkColumn() {X = chunk.X, Z = chunk.Z - 1}), chunk, o);
                if (n.LocalID == workingchunk.LocalID && pos == 0)
                {
                    //BOTTOM
                    nc = o.GenerateChunkColumn(new ChunkCoordinates() {X = chunk.X, Z = chunk.Z - 1}, true);
                    if (nc != null)
                    {
                        pos = 3;
                        h = CreateMapFrom2Chunks(chunk, nc, pos);
                    }
                }

                n = BiomeManager.GetBiome(o.getChunkRTH(new ChunkColumn() {X = chunk.X - 1, Z = chunk.Z}), chunk, o);
                if (n.LocalID == workingchunk.LocalID && pos == 0)
                {
                    //LEFT
                    nc = o.GenerateChunkColumn(new ChunkCoordinates() {X = chunk.X - 1, Z = chunk.Z}, true);
                    if (nc != null)
                    {
                        pos = 4;
                        h = CreateMapFrom2Chunks(chunk,
                            nc, pos);
                    }
                }

                if (pos == 0)
                {
                    Console.WriteLine("ERRRRRRRRRRRR NOOOOOOOOOO SMOOOOOOOOOOOOOOTHHHHHHHHH");
                }
                else
                {
                    var nh = SmoothMapV2(h);
                    printDisplayTable(nh);
                }

                if (pos == 1)
                {
                    int sx = chunk.
                }
            }


            // var points = new int[,];
            // int maxadjust = 20;
            // ChunkColumn[] cc = new ChunkColumn[8];
            // int a = 0;
            // cc[0] = openExperimentalWorldProvider.GenerateChunkColumn(new ChunkCoordinates()
            // {
            //     X = c.X - 1,
            //     Z = c.Z + 1
            // }, true);
            // cc[1] = openExperimentalWorldProvider.GenerateChunkColumn(new ChunkCoordinates()
            // {
            //     X = c.X,
            //     Z = c.Z + 1
            // }, true);
            // cc[2] = openExperimentalWorldProvider.GenerateChunkColumn(new ChunkCoordinates()
            // {
            //     X = c.X + 1,
            //     Z = c.Z + 1
            // }, true);
            // cc[3] = openExperimentalWorldProvider.GenerateChunkColumn(new ChunkCoordinates()
            // {
            //     X = c.X - 1,
            //     Z = c.Z
            // }, true);
            // cc[4] = openExperimentalWorldProvider.GenerateChunkColumn(new ChunkCoordinates()
            // {
            //     X = c.X + 1,
            //     Z = c.Z
            // }, true);
            // cc[5] = openExperimentalWorldProvider.GenerateChunkColumn(new ChunkCoordinates()
            // {
            //     X = c.X - 1,
            //     Z = c.Z - 1
            // }, true);
            // cc[6] = openExperimentalWorldProvider.GenerateChunkColumn(new ChunkCoordinates()
            // {
            //     X = c.X,
            //     Z = c.Z - 1
            // }, true);
            // cc[7] = openExperimentalWorldProvider.GenerateChunkColumn(new ChunkCoordinates()
            // {
            //     X = c.X - 1,
            //     Z = c.Z - 1
            // }, true);
            //
            // //TODO Create Gradiant Map of the 2 chunks
            // int ah = getAverageHeight(c);
            // int avgh = 0;
            // ChunkColumn workingchunk;
            // workingchunk = cc[1];
            // int[,] mainhm = new int[17,17]; 
            // int[,] whm = new int[17,17]; 
            // int[,] fhm = new int[33,33]; 
            // if (workingchunk != null)
            // {
            //     // //TOP = 1 // X+
            //     // for (var x = 0; x < 16; x++)
            //     // {
            //     //     for (var z = 0; z < 16; z++)
            //     //     {
            //     //         mainhm[x, z] = c.GetHeight(x, z);
            //     //         whm[x, z] = workingchunk.GetHeight(x, z);
            //     //     }
            //     // }
            //     // //Combine
            //     // for (var x = 0; x < 16; x++)
            //     // {
            //     //     for (var z = 0; z < 16; z++)
            //     //     {
            //     //         fhm[x, z+16] = mainhm[x, z];
            //     //         fhm[x, z] = whm[x, z];
            //     //     }
            //     // }
            //
            //     // int maxDropoff = 2;
            //     // for (var x = 0; x < 16; x++)
            //     // {
            //     //     for (var z = 0; z < 32-1; z++)
            //     //     {
            //     //         int ch = fhm[x, z];
            //     //         int h = fhm[x, z + 1];
            //     //         if()
            //     //         if(maxDropoff + h < ch)
            //     //     }
            //     // }         Vector3 d = new Vector3(-1,0,0);
            //     // for (var x = 0; x < 16; x++)
            //     // {
            //     //     for (var z = 0; z < 32; z++)
            //     //     {
            //     //         int lh = c.GetHeight(x, z + 1);
            //     //         int n = (int)GetNoise(x, z, .015f, 2);
            //     //         
            //     //     }
            //     // }
            // }
        }


        public int getAverageHeight(ChunkColumn c)
        {
            int avg = 0;
            for (var x = 0; x < 16; x++)
            for (var z = 0; z < 16; z++)
            {
                var h = c.GetHeight(x, z);
                avg = (avg + h) / 2;
            }

            return avg;
        }

        public static AdvancedBiome GetBiome(int biomeId)
        {
            return BiomeManager.GetBiome(biomeId);
        }

        private static readonly OpenSimplexNoise OpenNoise = new OpenSimplexNoise("a-seed".GetHashCode());


        public static float GetNoise(int x, int z, float scale, int max)
        {
            return (float) ((OpenNoise.Evaluate(x * scale, z * scale) + 1f) * (max / 2f));
        }
    }
}