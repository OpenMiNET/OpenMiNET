using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AStarNavigator;
using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Worlds;

namespace HeightSmootingTest
{
    class Program
    {
        public static int MASTERX = 32 * 2;
        public static int MASTERZ = 32 * 2;

        // private static readonly Bitmap image = new Bitmap(MASTERX * 16, MASTERZ * 16);
        private static readonly Bitmap image = new Bitmap(MASTERX, MASTERZ);
        private static readonly Bitmap b4image = new Bitmap(MASTERX, MASTERZ);

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var r = CreateRandomMap(MASTERX,150,80);
            for (var ax = 0; ax < MASTERX; ax++)
            {
                for (var az = 0; az < MASTERZ; az++)
                {
                    // Console.WriteLine($"{ax} || {az}");
                    var c = getHieghtToColor2(r[ax, az], 127);
                    PlotPixel2(ax,az,c[0],c[1],c[2]);
                }
            }
            b4image.Save("D:\\B4-HEIGHTSMOOTINGTEST.png");
            printDisplayTable(r,"B4 Tansformation");
            var m = SmoothMapV3(r);
            // printDisplayTable(m,"After Tansformation");
            for (var ax = 0; ax < MASTERX; ax++)
            {
                for (var az = 0; az < MASTERZ; az++)
                {
                    var c = getHieghtToColor2(m[ax, az], 150);
                    PlotPixel(ax,az,c[0],c[1],c[2]);
                }
            }
            
            
            image.Save("D:\\HEIGHTSMOOTINGTEST.png");
        }
        
        private static void PlotPixel(int x, int y, byte redValue,
            byte greenValue, byte blueValue)
        {
            image.SetPixel(x, y, Color.FromArgb(255, redValue, greenValue, blueValue));
        }
        private static void PlotPixel2(int x, int y, byte redValue,
            byte greenValue, byte blueValue)
        {
            b4image.SetPixel(x, y, Color.FromArgb(255, redValue, greenValue, blueValue));
        }

        public static void SmoothToNeighbor(int x, int z, int xo,int zo, int max, int[,] map)
        {
            if (x+xo == 0 || x+xo == 16 || z+zo == 0 || z+zo == 16) return;
            int ch = map[x, z];
            //Top
            var n = getNeighbor(x, z, xo,zo,max, map);
            if (n != -1)
            {
                // int d = Math.Abs(ch - n);
                int d = new Random().Next(0,3);
                if (n > ch)
                {
                    n = ch + d;
                }
                else
                {
                    n = ch - d;
                }

                map[x+xo , z+zo] = n;
            }
        }
        
        public static byte[] getHieghtToColor2(int height, int max = 180)
        {
            var color = new byte[3] {0, 0, 0};
            int[] Red = {255, 0, 0};
            int[] Green = {0, 255, 0};
            int[] Blue = {0, 0, 255};
            var v = Math.Min(height, max) / (float) max;
            var nv = Math.Abs(v - 1);
            color[0] = (byte) (255 * nv);
            color[1] = (byte) (255 * v);

            return color;
        }
        
        public static int[,] SmoothMapV2(int[,] map)
        {
            // int[,]  newmap = new int[map.GetLength(0),map.GetLength(1)];
            int[,]  newmap = map;
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int z = 0; z < map.GetLength(1); z++)
                {
                    if (x == 0 || x == 16 || z == 0 || z == 16) continue;
                    //TopLeft
                    SmoothToNeighbor(x,z,-1,1,map.GetLength(1),newmap);
                    //TopMiddle
                    SmoothToNeighbor(x,z,0,1,map.GetLength(1),newmap);
                    //TopRight
                    SmoothToNeighbor(x,z,1,1,map.GetLength(1),newmap);
                    
                    //MIDLE
                    
                    
                    //MidLeft
                    SmoothToNeighbor(x,z,-1,0,map.GetLength(1),newmap);
                    //MidRight
                    SmoothToNeighbor(x,z,1,0,map.GetLength(1),newmap);
                    
                    //BOTTOM
                    //BottomLeft
                    SmoothToNeighbor(x,z,-1,-1,map.GetLength(1),newmap);
                    //BottomMiddle
                    SmoothToNeighbor(x,z,0,-1,map.GetLength(1),newmap);
                    //BottomRight
                    SmoothToNeighbor(x,z,1,-1,map.GetLength(1),newmap);
                    
                }
            }

            return newmap;
        }
        public static int[,] SmoothMapV3(int[,] map)
        {
            int[,]  newmap = new int[map.GetLength(0),map.GetLength(1)];
            // int[,]  newmap = map;
            for (int x = 0; x < map.GetLength(0); x++)
            {
                int th = map[x, map.GetLength(1)-1];
                for (int z = 0; z < map.GetLength(1); z++)
                {
                    if (x == 0 || x == 16 || z == 0 || z == 16)
                    {
                        newmap[x, z] = map[x, z];
                        continue;
                    }

                    int lh = map[x, z-1];
                    int ch = map[x, z];
                    if (ch > th)
                    {
                        //SUBTRACT
                        // int nch = (int) Math.Floor((ch + th) / 2f);
                        int nch = ch - 1;
                        if (nch >= lh) nch = lh - 1;
                        newmap[x, z] = nch;
                    }
                    else if (ch == th)
                    {
                        newmap[x, z] = ch;
                        
                    }else
                    {  int nch = ch - 1;
                        if (nch >= lh) nch = lh - 1;
                        newmap[x, z] = nch;
                        //ADD
                    }
                }
            }

            return newmap;
        }

        public static int getNeighbor(int x, int z, int xo,int zo, int max, int[,] map)
        {
            if (x + xo >= max || x + xo < 0) return -1;
            if (z + zo >= max || z + zo < 0) return -1;
            return map[x + xo, z+zo];
        }
        //
        // public int[,] SmoothMapV1(int[,] map)
        // {
        //     int maxdiff = 2;
        //     int a = 0;
        //     int[,] hm = new int[7, 7];
        //     HeightChunk hc = new HeightChunk(map);
        //     for (int i = 0; i < 3; i++)
        //     {
        //         HeightSubChunk sc = hc.Chunks[i];
        //         for (int ii = 0; ii < 3; ii++)
        //         {
        //             HeightSubSubChunk ssc = sc.Chunks[i];
        //             for (int iii = 0; iii < 3; iii++)
        //             {
        //                 HeightSubSubSubChunk sssc = ssc.Chunks[i];
        //                 int x = (int) Math.Floor(a / 8f);
        //                 int z = a % 8;
        //                 int avg = sssc.getAverage();
        //                 int q = 0;
        //                 for (int j = 0; j < 4; j++)
        //                 {
        //                     var aa = sssc.v[j];
        //                     var aaa = Math.Abs(aa - avg);
        //                     if (aaa > maxdiff)
        //                     {
        //                         if (avg > maxdiff)
        //                         {
        //                             sssc.v[j] = aa + aaa;
        //                         }
        //                         else
        //                             sssc.v[j] = aa - aaa;
        //                     }
        //                 }
        //
        //
        //                 // for (int iiii = 0; iiii < 3; iiii++)
        //                 // {
        //                 //     var ssssc = sssc.Chunks;
        //                 // }
        //                 a++;
        //             }
        //         }
        //     }
        // }

        public static int[,] CreateRandomMap(int size, int maxh, int minh)
        {
            int[,] m = new int[size, size];
            for (int x = 0; x < m.GetLength(0); x++)
            {
                for (int z = 0; z < m.GetLength(1); z++)
                {
                    m[x, z] = (int) GetNoise(x, z, 0.015f, 127) + (new Random()).Next(0, 20);
                    ;
                }
            }

            return m;
        }

        private static readonly OpenSimplexNoise OpenNoise = new OpenSimplexNoise("a-seed".GetHashCode());


        public static float GetNoise(int x, int z, float scale, int max)
        {
            return (float) (OpenNoise.Evaluate(x * scale, z * scale) + 1f) * (max / 2f);
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
    }
}