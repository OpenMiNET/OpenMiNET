using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using MiNET.Blocks;
using MiNET.Net;
using MiNET.Utils;
using MiNET.Worlds;
using Org.BouncyCastle.Tsp;

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

            // Console.WriteLine($"CHUNK SMOOTHING OF {chunk.X} {chunk.Z} TOOK {t.Elapsed}");
            return chunk;
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
     
            // SmoothChunk(openExperimentalWorldProvider,chunk,rth);
            // Console.WriteLine($"CHUNK POPULATION OF {chunk.X} {chunk.Z} TOOK {t.Elapsed}");
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

        public void SetHeightMapToChunks(ChunkColumn c, ChunkColumn[] ca, int[,] map)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            for (int z = 0; z < map.GetLength(1); z++)
            {
                int cnx = (int) Math.Floor(x / 16f);
                int cnz = (int) Math.Floor(z / 16f);
                int cn = cnx + (cnz * 3);
                // if()
                // if (cn >= 6) cn--;
                ChunkColumn cc;
                if (cn == 4)
                {
                    cc = c;
                    // continue;
                }
                else
                {
                    if (cn > 4) cn--;
                    cc = ca[cn];
                }

                int rx = x % 16;
                int rz = z % 16;
                int h = map[x, z];
                int rzz = (15 - rz);
                // int rzz = ( rz);
                // int rxx = ( 15-rx);
                int rxx = rx;
                // map[x, z] = cc.GetHeight(rx, rz);

                // Console.WriteLine($"{x} AND {z} GAVE ({cnx} AND {cnz}) {cn} AND CHUNK {ca[cn]}");

                // if (x == 0 || z == map.GetLength(0) - 1 || z == 0 || z == map.GetLength(1) - 1)
                // {
                // Console.WriteLine(
                //     $"{x} {z} > GAVE ({cnx} AND {cnz}) CN VAL {cn} ||  {rx} {rz} ({rzz}) >======== {h}");
                for (int y = 1; y < 255; y++)
                {
                    if (y < h - 1)
                    {
                        if (cc.GetBlockId(rxx, y, rzz) == 0) cc.SetBlock(rxx, y, rzz, new Stone());
                    }
                    // else if (cc.GetBlockId(rx, y, rz) == 0) break;
                    else if (y == h - 1)
                    {
                        // if (x == 0 || z == map.GetLength(0) - 1 || z == 0 || z == map.GetLength(1) - 1)
                        //     cc.SetBlock(rxx, y, rzz, new EmeraldBlock());
                        /*else*/ cc.SetBlock(rxx, y, rzz, new Grass());
                    }
                    else
                    {
                        if (cc.GetBlockId(rxx, y, rzz) == 0 || cc.GetBlockId(rxx, y, rzz) == new Wood().Id) break;
                        cc.SetBlock(rxx, y, rzz, new Air());
                    }

                    // if (y < h)
                    // {
                    //     if(cc.GetBlockId(rx,y,rz) == 0)cc.SetBlock(rx,y,rz , new CoalBlock());
                    // }
                    // else
                    // {
                    //     if (cc.GetBlockId(rx, y, rz) == 0) break;
                    //     cc.SetBlock(rx,y,rz , new Air());
                    // }
                    // }
                }

                cc.SetHeight(rxx, rzz, (short) h);
            }
        }

        public int[,] CreateMapFrom8Chunks(ChunkColumn c, ChunkColumn[] ca)
        {
            var map = new int[16 * 3, 16 * 3];
            for (int z = 0; z < map.GetLength(1); z++)
            for (int x = 0; x < map.GetLength(0); x++)
            {
                int rx = x % 16;
                int rz = 15 - z % 16;
                int cnx = (int) Math.Floor(x / 16f);
                int cnz = (int) Math.Floor(z / 16f);
                int cn = cnx + (cnz * 3);
                // if()
                // if (cn >= 6) cn--;
                ChunkColumn cc;
                if (cn == 4)
                {
                    cc = c;
                }
                else
                {
                    if (cn > 4) cn--;
                    cc = ca[cn];
                }
                // Console.WriteLine($"{x} AND {z} GAVE ({cnx} AND {cnz}) {cn} AND CHUNK {ca[cn]}");

                map[x, z] = cc.GetHeight(rx, rz);
                // for (int yy = 0; yy < 254; yy++)
                // {
                //     if (cc.GetBlockId(rx, yy, rz) == 0)
                //     {
                //
                //         if(map[x,z] != yy)Console.WriteLine($"ERROR {map[x, z]} WAS SET BUT IS {yy}");
                //         map[x, z] = yy;
                //         break;
                //     }
                // }
            }


            return map;
        }

        public class ChunkColumHeightMap
        {
            private int[] Map;

            public ChunkColumHeightMap(int xlenght, int zlenght)
            {
            }
        }

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

        public int[,] SmoothMapV3(int[,] map)
        {
            int[,] newmap = new int[map.GetLength(0), map.GetLength(1)];
            // int[,]  newmap = map;

            //SMooth BORDER
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int z = 0; z < map.GetLength(1); z++)
                {
                    if (x == 0 || x == map.GetLength(0) - 1 || z == 0 || z == map.GetLength(1) - 1)
                    {
                        if ((x == 0 || x == map.GetLength(0) - 1) && (z == 0 || z == map.GetLength(1) - 1))
                        {
                            continue;
                        }

                        int lv = -1;
                        int nv = -1;
                        if (z == 0)
                        {
                            lv = map[x - 1, z];
                            nv = map[x + 1, z];
                        }
                        else if (z == map.GetLength(1) - 1)
                        {
                            lv = map[x - 1, z];
                            nv = map[x + 1, z];
                        }
                        else if (x == 0)
                        {
                            lv = map[x, z - 1];
                            nv = map[x, z + 1];
                        }
                        else if (x == map.GetLength(0) - 1)
                        {
                            lv = map[x, z - 1];
                            nv = map[x, z + 1];
                        }

                        int cv = map[x, z];
                        int a = (lv + nv) / 2;
                        int dv = (a - cv);
                        if (dv > 1)
                        {
                            if (lv > nv)
                                a = lv - 1;
                            else
                                a = lv + 1;
                        }
                        else if (dv < -1)
                        {
                            if (lv < nv)
                                a = lv + 1;
                            else
                                a = lv - 1;
                        }

                        int fv = a;
                        // Console.WriteLine($"{x} {z} => {cv} ||| LV{lv} NV{nv} | A{a} | DV{dv} | {fv}");
                        map[x, z] = fv;
                        // if (x == 0)
                        // {
                        //     map[x, z] = (int) Lerp(map[0, 0], map[0, map.GetLength(1) - 1],
                        //         (float) z / map.GetLength(1));
                        // }
                        // else if (x == map.GetLength(0) - 1)
                        // {
                        //     map[x, z] = (int) Lerp(map[map.GetLength(0) - 1, 0],
                        //         map[map.GetLength(0) - 1, map.GetLength(1) - 1],
                        //         (float) z / map.GetLength(1));
                        // }
                        // else if (z == 0)
                        // {
                        //     map[x, z] = (int) Lerp(map[0, 0], map[map.GetLength(0) - 1, 0],
                        //         (float) x / map.GetLength(0));
                        // }
                        // else if (z == map.GetLength(0) - 1)
                        // {
                        //     map[x, z] = (int) Lerp(map[0,map.GetLength(1) - 1],
                        //         map[map.GetLength(0) - 1, map.GetLength(1) - 1], 
                        //         (float) x / map.GetLength(1));
                        // }

                        // float nhx = Lerp(map[0, z], map[map.GetLength(0) - 1, z], (float) x / (map.GetLength(0) - 1));
                        // float nhz = Lerp(map[x, 0], map[x, map.GetLength(1) - 1], (float) z / (map.GetLength(1) - 1));
                        //
                        // // map[x, z] =
                        //     int v = map[x, z];
                    }
                }
            }

            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int z = 0; z < map.GetLength(1); z++)
                {
                    if (x == 0 || x == map.GetLength(0) - 1 || z == 0 || z == map.GetLength(1) - 1)
                    {
                        newmap[x, z] = map[x, z];
                        continue;
                    }

                    int cv = map[x, z];
                    int lvx = map[x - 1, z];
                    int lvz = map[x, z - 1];
                    int nvx = map[x + 1, z];
                    int nvz = map[x, z + 1];
                    int c1 = map[x - 1, z + 1];
                    int c2 = map[x + 1, z + 1];
                    int c3 = map[x - 1, z - 1];
                    int c4 = map[x - 1, z - 1];
                    int tvx = map[map.GetLength(0) - 1, z];
                    int tvz = map[x, map.GetLength(1) - 1];
                    int lndx = nvx - lvx;
                    int lndz = nvz - lvz;
                    int lnax = (nvx + lvx) / 2;
                    //Smooth Z
                    int lnaz = (nvz + lvz + lnax) / 3;
                    newmap[x, z] = lnaz;
                    // Console.WriteLine($" OK >> LAN> {lnax} {lnaz}");
                    // map[x, z] = lnax;
                    float lnaxm = lnax / (float) cv;
                    float lnazm = lnaz / (float) cv;
                    // int m =  
                    int lna = (int) Math.Ceiling((lnax + lnaz + c1 + c2 + c3 + c4) / 6f);
                    // if (nvx > lna+1)
                    // {
                    //     lna = lvx + 1;
                    // }else if (nvz > lna + 1)
                    // {
                    //     lna = lvz + 1;
                    // }
                    // if (lvx + 1 < lna)
                    // { - cv;
                    //     lna = lvx + 1;
                    //     if(nvx < lna)
                    //         lna = nvx - 1;
                    // }
                    // if (lvz + 1 < lna)
                    // {
                    //     lna = lvz + 1;
                    //     if(nvz < lna)
                    //         lna = nvz - 1;
                    // }

                    //X AXIS
                    // float nhx = Lerp(map[0, z], map[map.GetLength(0) - 1, z], (float) x / (map.GetLength(0) - 1));
                    // //Z AXIS
                    // float nhz = Lerp(map[x, 0], map[x, map.GetLength(1) - 1], (float) z / (map.GetLength(1) - 1));
                    // newmap[x, z] = (int) Math.Floor((nhx + nhz) / 2f);
                    // newmap[x, z] = lna;

                    // Console.WriteLine($"LERPING CHHUNK FROM {map[x,0]} TO {map[x,map.GetLength(1) - 1]} WITH {(float)z / map.GetLength(1)} ====== {nhz}");
                    // float nhx = Lerp(map[0, z], map[map.GetLength(0) - 1, z], x / map.GetLength(0));
                    // float nhz = Lerp(map[x, 0], map[x, map.GetLength(1) - 1], z / map.GetLength(1));
                    // newmap[x, z] = (int) Math.Floor(((float)(x+z)/(map.GetLength(0)+map.GetLength(1)))*100 + 50);
                    // newmap[x, z] = (int) Math.Floor(( nhz) );
                    // newmap[x, z] = BiomeQualifications.baseheight + x+z;
                }
            }

            return newmap;
        }

        public int[,] SmoothMapV2(int[,] map)
        {
            int[,] newmap = new int[map.GetLength(0), map.GetLength(1)];
            // int[,]  newmap = map;
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int z = 0; z < map.GetLength(1); z++)
                {
                    if (x == 0 || x == map.GetLength(0) || z == 0 || z == map.GetLength(1))
                    {
                        newmap[x, z] = map[x, z];
                        continue;
                    }

                    float nhx = Lerp(map[x, 0], map[x, map.GetLength(1) - 1], z / map.GetLength(1));
                    float nhz = Lerp(map[0, z], map[map.GetLength(0) - 1, z], x / map.GetLength(0));
                    // float nhx = Lerp(map[0, z], map[map.GetLength(0) - 1, z], x / map.GetLength(0));
                    // float nhz = Lerp(map[x, 0], map[x, map.GetLength(1) - 1], z / map.GetLength(1));
                    newmap[x, z] = (int) Math.Floor((nhx + nhz) / 2f);
                }
            }

            return newmap;
        }

        public float Lerp(int firstFloat, int secondFloat, float by)
        {
            return firstFloat * (1 - by) + secondFloat * by;
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
            mastertitle = "";
            for (var z = 0; z < mz; z++)
            {
                var s = "";
                for (var x = 0; x < mx; x++)
                {
                    s += $"{table[x, z]},";
                    // if (x + 1 == mx) s += "||";
                }

                mastertitle += "\n" + s;
                Console.WriteLine(s);
            }

            Console.WriteLine("==========================================================");
            System.IO.File.WriteAllText($@"D:\MINET\{title}.csv", mastertitle);
        }


        public void FormatChunk(int x, int z, int xo, int zo, int dif, ChunkColumn chunk, ChunkColumn nc)
        {
            if (x < 16 && z < 16)
            {
                for (var y = 20; y < 255; y++)
                {
                    if (y > dif)
                    {
                        if (chunk.GetBlockId(x, y, z) == 0) break;
                        chunk.SetBlock(x, y, z, new Air());
                    }
                    else if (y == dif)
                    {
                        chunk.SetBlock(x, y, z, new StainedGlass()
                        {
                            Color = "orange"
                        });
                    }
                    else
                    {
                        // if (chunk.GetBlockId(x, y, z) == 0)
                        // {
                        chunk.SetBlock(x, y, z, new Stone());
                        // }
                    }
                }
            }
            else
            {
                for (var y = 20; y < 255; y++)
                {
                    if (y > dif)
                    {
                        if (nc.GetBlockId(x - xo, y, z - zo) == 0) break;
                        nc.SetBlock(x - xo, y, z - zo, new Air());
                    }
                    else if (y == dif)
                    {
                        nc.SetBlock(x - xo, y, z - zo, new StainedGlass()
                        {
                            Color = "orange"
                        });
                    }
                    else
                    {
                        // if (chunk.GetBlockId(x, y, z) == 0)
                        // {
                        nc.SetBlock(x - xo, y, z - zo, new Stone());
                        // }
                    }
                }
            }
        }


        public static int max = 0;

        public void SmoothChunk(OpenExperimentalWorldProvider o, ChunkColumn chunk, float[] rth)
        {
            //Smooth Biome

            if (BorderChunk)
            {
                max++;
                chunk.SetBlock(8, 110, 8, new EmeraldBlock());
                AdvancedBiome n;
                var nc = new ChunkColumn();
                var pos = 0;
                int[,] h = null;
                var i = -1;

                if (BorderType != 0)
                {
                    ChunkColumn[] chunks = new ChunkColumn[8];
                    int ab = 0;
                    chunks[0] = o.GenerateChunkColumn2(new ChunkCoordinates {X = chunk.X - 1, Z = chunk.Z + 1}, false);
                    chunks[1] = o.GenerateChunkColumn2(new ChunkCoordinates {X = chunk.X, Z = chunk.Z + 1}, false);
                    chunks[2] = o.GenerateChunkColumn2(new ChunkCoordinates {X = chunk.X + 1, Z = chunk.Z + 1}, false);
                    chunks[3] = o.GenerateChunkColumn2(new ChunkCoordinates {X = chunk.X - 1, Z = chunk.Z}, false);
                    chunks[4] = o.GenerateChunkColumn2(new ChunkCoordinates {X = chunk.X + 1, Z = chunk.Z}, false);
                    chunks[5] = o.GenerateChunkColumn2(new ChunkCoordinates {X = chunk.X - 1, Z = chunk.Z - 1}, false);
                    chunks[6] = o.GenerateChunkColumn2(new ChunkCoordinates {X = chunk.X, Z = chunk.Z - 1}, false);
                    chunks[7] = o.GenerateChunkColumn2(new ChunkCoordinates {X = chunk.X + 1, Z = chunk.Z - 1}, false);
                 


                    h = CreateMapFrom8Chunks(chunk, chunks);
                    var nh = SmoothMapV3(h);
                    nh = SmoothMapV4(nh);

                    // printDisplayTable(h, $"C{chunk.X}{chunk.Z}Pre");
                    // printDisplayTable(nh, $"C{chunk.X}{chunk.Z}Post");

                    SetHeightMapToChunks(chunk, chunks, nh);
                }

                // if (pos == 0) 
                // {
                //     Console.WriteLine("ERRRRRRRRRRRR NOOOOOOOOOOaaaaaaaa SMOOOOOOOOOOOOOOTHHHHHHHHH");
                // }
                // else
                // {
                //     // var nh = SmoothMapV2(h);
                //     // printDisplayTable(nh);
                //
                //     // chunk.SetBlock(8, 109, 8, new Netherrack());
                //     // chunk.SetBlock(8, 109 - pos, 8, new EmeraldBlock());
                //     Console.WriteLine($"ABOUT TO SMOOTH BUT POS={pos} NC={nc} ");
                // }
                //
                // if (pos != 0 && nc != null)
                // {
                //     chunk.SetBlock(8, 111, 8, new RedstoneBlock());
                //     Console.WriteLine($"SMOOTHING CHUNK {chunk.X} {chunk.Z}");
                //     var nh = SmoothMapV3(h);
                //
                //     printDisplayTable(nh,$"{chunk.X} {chunk.Z}");
                //     
                //
                //     // var xx = 0;
                //     // var zz = 0;
                //     Console.WriteLine($" X:{nh.GetLength(0)} ||| Z:{nh.GetLength(1)}");
                //     for (var z = 0; z < nh.GetLength(1); z++)
                //     {
                //         for (var x = 0; x < nh.GetLength(0); x++)
                //         {
                //             // Console.WriteLine($"STARTING ON {x} ::: {z}");
                //             // for (var z = sz; z < stopz; z++)
                //             // {
                //             //     for (var x = sx; x < stopx; x++)
                //             //     {
                //             var dif = nh[x, z];
                //             // dif = h[x, z];
                //
                //             // for (var y = 50; y < 255; y++)
                //             // {
                //             //     if (y >= dif)
                //             //         o.Level.SetBlock(new Air()
                //             //         {
                //             //             Coordinates = new BlockCoordinates(sx+x,y,sz+z)
                //             //         });
                //             //     if (o.Level.GetBlock(new PlayerLocation(x, y, z)).Id == 0) break;
                //             // }
                //
                //             if (pos == 1 || pos == 3)
                //             {
                //                 if (pos == 1)
                //                 {
                //                     chunk.SetBlock(8, 110, 8 + 1, new Furnace());
                //                     FormatChunk(x, z, 0, 16, dif, chunk, nc);
                //                 }
                //                 else if (pos == 3)
                //                 {
                //                     chunk.SetBlock(8, 110, 8 - 1, new Furnace());
                //                     FormatChunk(x, z, 0, 16, dif, nc, chunk);
                //                 }
                //             }
                //             else if (pos == 2 || pos == 4)
                //             {
                //                 // if(x == 0 || x == 16 )
                //                 if (pos == 2)
                //                 {
                //                     chunk.SetBlock(8 + 1, 110, 8, new Furnace());
                //                     FormatChunk(x, z, 16, 0, dif, chunk, nc);
                //                 }
                //                 else if (pos == 4)
                //                 {
                //                     chunk.SetBlock(8 - 1, 110, 8, new Furnace());
                //                     FormatChunk(x, z, 16, 0, dif, nc, chunk);
                //                 }
                //             }
                //
                //             // xx++;
                //         }
                //
                //         // xx = 0;
                //         // zz++;
                //     }
                // }

                Console.WriteLine("=========================||======================");
            }
        }

        private int[,] SmoothMapV4(int[,] map)
        {
            int[,] newmap = map;
            for (int x = 1; x < map.GetLength(0) - 2; x++)
            {
                var zstrip = SmoothStrip(Fillstripz(1,map.GetLength(1)-2,x,map));
                int cnt = 0;
                for (int z = 1; z < map.GetLength(1) - 2; z++)
                {
                    map[x, z] = zstrip[cnt];
                    cnt++;
                }
                // for (int z = 1; z < map.GetLength(1) - 2; z++)
                // {
                //     
                //     
                //     
                //     int cv = map[x, z];
                //     int lvx = map[x - 1, z];
                //     int nvx = map[x + 1, z];
                //     int d = -lvx + nvx;
                //     int v1 = lvx - cv;
                //     int v2 = cv - nvx;
                //     Console.WriteLine($"{x} {z} ||>> {lvx} | {cv} | {nvx} ||| {v1} VS {v2} TO");
                //     if (v1 >= 1 || v2 >= 1 || v1 <= -1 || v2 <= -1)
                //         // if ((v1 >= 1 || v2 >= 1) && (v1 <= -1 || v2 <= -1))
                //     {
                //         //Between -1 & 1
                //         newmap[x, z] = map[x, z];
                //         continue;
                //     }
                //
                //     newmap[x, z] = lvx + ((d >= 0 ? 1 : -1));
                //     //Z SMOOTH NOW
                // }
            }
            for (int z = 1;z < map.GetLength(1) - 2; z++)
            {
                var xstrip = SmoothStrip(Fillstripx(1,map.GetLength(1)-2,z,map));
                int cnt = 0;
                for (int x = 1; x < map.GetLength(0) - 2; x++)
                {
                    map[x, z] = xstrip[cnt];
                    cnt++;
                }
            }
            // for (int x = 1; x < map.GetLength(0) - 2; x++)
            // {
            //     for (int z = 1; z < map.GetLength(1) - 2; z++)
            //     {
            //         int cv = map[x, z];
            //         int lvz = map[x , z-1];
            //         int nvz = map[x , 1+z];
            //         int d = -lvz + nvz;
            //         int v1 = lvz - cv;
            //         int v2 = cv - nvz;
            //         Console.WriteLine($"{x} {z} ||>> {lvz} | {cv} | {nvz} ||| {v1} VS {v2} TO");
            //         if (v1 >= 1 || v2 >= 1 || v1 <= -1 || v2 <= -1)
            //             // if ((v1 >= 1 || v2 >= 1) && (v1 <= -1 || v2 <= -1))
            //         {
            //             //Between -1 & 1
            //             // newmap[x, z] = map[x, z];
            //             continue;
            //         }
            //
            //         newmap[x, z] = lvz + ((d >= 0 ? 1 : -1));
            //         //Z SMOOTH NOW
            //     }
            // }

            return newmap;
        }

        private int[] SmoothStrip(int[] fillstripz)
        {
            for (int i = 0; i < fillstripz.Length-1; i++)
            {
                if (i == 0 || i == fillstripz.Length - 1) continue;
                int lv = fillstripz[i - 1];
                int nv = fillstripz[i + 1];
                int v = fillstripz[i];
                //DOWN OR UP
                int du = nv - lv;
                //UP
                if (du > 0)
                {
                    v = lv + 1;
                }else if (du < 0)
                {
                    v = lv - 1;
                }
                else v = lv;

                fillstripz[i] = v;
            }

            return fillstripz;
        }

        private int[] Fillstripx(int i, int getLength, int z, int[,] map)
        {
            List<int> strip = new List<int>();
            for (int a = i; a < getLength; a++)
            {
                strip.Add(map[a,z]);
            }
            return strip.ToArray();
        }
        private int[] Fillstripz(int i, int getLength, int x, int[,] map)
        {
            List<int> strip = new List<int>();
            for (int a = i; a < getLength; a++)
            {
                strip.Add(map[x,a]);
            }
            return strip.ToArray();
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
            return (heightnoise.GetNoise(x, z) + 1) * (max / 2f);
            // return (float) ((OpenNoise.Evaluate(x * scale, z * scale) + 1f) * (max / 2f));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="openExperimentalWorldProvider"></param>
        /// <param name="chunk"></param>
        /// <param name="rth"></param>
        /// <returns></returns>
        public virtual ChunkColumn GenerateSurfaceItems(OpenExperimentalWorldProvider openExperimentalWorldProvider, ChunkColumn chunk, float[] rth)
        {
            
            return chunk;
        }
    }
}