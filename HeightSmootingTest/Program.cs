using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AStarNavigator;
using MiNET.Utils;

namespace HeightSmootingTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var r = CreateRandomMap(4,150,80);
            printDisplayTable(r,"B4 Tansformation");
            
            printDisplayTable(SmoothMapV2(r),"After Tansformation");
        }

        public static void SmoothToNeighbor(int x, int z, int xo,int zo, int max, int[,] map)
        {
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
        
        public static int[,] SmoothMapV2(int[,] map)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int z = 0; z < map.GetLength(1); z++)
                {
                    //TopLeft
                    SmoothToNeighbor(x,z,-1,1,map.GetLength(1),map);
                    //TopMiddle
                    SmoothToNeighbor(x,z,0,1,map.GetLength(1),map);
                    //TopRight
                    SmoothToNeighbor(x,z,1,1,map.GetLength(1),map);
                    
                    //MIDLE
                    
                    
                    //MidLeft
                    SmoothToNeighbor(x,z,-1,0,map.GetLength(1),map);
                    //MidRight
                    SmoothToNeighbor(x,z,1,0,map.GetLength(1),map);
                    
                    //BOTTOM
                    //BottomLeft
                    SmoothToNeighbor(x,z,-1,-1,map.GetLength(1),map);
                    //BottomMiddle
                    SmoothToNeighbor(x,z,0,-1,map.GetLength(1),map);
                    //BottomRight
                    SmoothToNeighbor(x,z,1,-1,map.GetLength(1),map);
                    
                }
            }

            return map;
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