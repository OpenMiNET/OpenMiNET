using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using MiNET.Blocks;
using MiNET.Worlds;
using OpenAPI.World;
using Org.BouncyCastle.Crypto.Parameters;

namespace TestingWorldGenConsoleReal
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // var x = Console.Read();
            // Console.WriteLine("+++++++++++++++++++++++++++++++++++ "+x);
            // if (x == 48)
            // {
                new BiomeChunkTester().start();
                // BiomeTester.start();
            // }
            // else
            // {
            //     Console.WriteLine("Hello World!");
            //     Console.WriteLine("Genertating 16 X 16 (256 Chunks) Chunk Radius to an Image!");
            //     start();
            // }
        }

        public static int MASTERX = 35 * 5*16;
        public static int MASTERZ = 35 * 5*16;


        public static void CalcuateHeightImage(List<ChunkColumn> ccl)
        {
            int max = 0;
            foreach (var ccc in ccl)
            {
                for (var ax = 0; ax < 16; ++ax)
                {
                    for (var az = 0; az < 16; ++az)
                    {
                        int i = ccc.GetHeight(ax, az);
                        if (i > max)
                        {
                            max = i;
                        }
                    }
                }
            }

            // var height = new int[256, 256];
            foreach (var cc in ccl)
            {
                for (var ax = 0; ax < 16; ++ax)
                {
                    for (var az = 0; az < 16; ++az)
                    {
                        var fx = cc.X * 16 + ax;
                        var fz = cc.Z * 16 + az;
                        var mh = 0;
                        mh = cc.GetHeight(ax, az);
                        // var b1 = cc.GetBlockObject(ax, mh, az);
                        // var b2 = cc.GetBlockObject(ax, mh + 1, az);
                        // var b3 = cc.GetBlockObject(ax, mh - 1, az);

                        // for (var ay = 255; ay > 0; ay--)
                        // {
                        //     var bid = cc.GetBlockId(ax, ay, az);
                        //     if (bid == 0)
                        //     {
                        //         Console.WriteLine($"TESTTTAAA|||| {ax} {ay} {az} ||| {bid} || {mh}");
                        //         mh = ay;
                        //         break;
                        //     }
                        // }


                        // height[fx, fz] = mh;

                        // Console.WriteLine(mh + "<<<<<<<<<<");


                        // Console.WriteLine(mh + $"<<<<<<<<<< {r}||{g}||{b}|||||||||||||||{a}");
                        // PlotPixel(fx, fz, r, g, b);
                        var v = getHieghtToColor2b(cc.GetBlockObject(ax, mh, az), mh, max);
                        // var v = getHieghtToColor2(mh, max);
                        PlotPixel(fx, fz, (byte) v[0], (byte) v[1], (byte) v[2]);
                        if (az == 0 || ax == 0) PlotPixel(fx, fz, 200, 200, 200);
                        // if (b1.Id == 9 || b1.Id == 8 || b2.Id == 9 || b2.Id == 8 || b3.Id == 9 || b3.Id == 8)
                        // PlotPixel(fx, fz, 0, 0, 200);
                        // if(az == 0 || ax == 0)PlotPixel(fx,fz,255,255,255);


                        // for (var ay = 0; ay < 256; ay++)
                        // {
                        //     var b = cc.
                        // }
                    }
                }
            }

            image.Save("D:\\WORLDGEN.png");
        }

        public static int[] getHieghtToColor(int height)
        {
            int[] color = new int[3] {0, 0, 0};
            int a;
            if (height < 50)
            {
                a = (height / 50);
                color[0] = a * 101;
                color[1] = a * 61;
            }
            else if (height < 100)
            {
                a = (height - 50 / 50);
                color[0] = a * 255;
                color[1] = a * 219;
            }
            else if (height < 150)
            {
                a = (height - 100 / 50);
                color[0] = a * 140;
                color[1] = a * 255;
            }
            else if (height < 200)
            {
                a = (height - 150 / 50);
                color[0] = a * 13;
                color[1] = a * 102;
            }
            else if (height < 250)
            {
                a = (height - 50 / 50);
                color[0] = a * 17;
                color[1] = a * 236;
                color[2] = a * 252;
            }
            else
            {
                color[0] = 68;
                color[1] = 17;
                color[2] = 252;
            }

            return color;
        }

        public static int[] getHieghtToColor2a(int height, int max = 180)
        {
            int[] color = new int[3] {0, 0, 0};

            int a;
            int aa;
            if (height < 127)
            {
                a = ((height / 127));
                aa = (a * -1) + 127;
                color[0] = aa * 255;
                color[2] = a * 50;
            }
            else
            {
                a = ((height - 127 / 127));
                aa = (a * -1) + 127;
                color[0] = a * 50;
                color[2] = aa * 255;
            }

            return color;
        }

        public static int[] getHieghtToColor2b(Block cc, int height, int max = 180)
        {
            int[] color = new int[3] {0, 0, 0};
            int[] Red = new int[] {255, 0, 0};
            int[] Green = new int[] {0, 255, 0};
            int[] Blue = new int[] {0, 0, 255};
            float v = Math.Min(height, max) / (float) max;
            float nv = Math.Abs(v - 1);
            color[0] = (int) (255 * nv);
            color[1] = (int) (255 * v);
            if (v > 5f / 256)
            {
                var b = cc.Id;
                if (b == 223)
                {
                    color = new[] {60, 50, 168};
                    //GreenGlazedTerracotta
                }
                else if (b == 152)
                {
                    //RedstoneBlock
                    color = new[] {131, 50, 168};
                }
                else if (b == 179)
                {
                    //RedSandstone
                    color = new[] {169, 107, 50};
                }
                else if (b == 9)
                {
                    color = new[] {50, 151, 168};
                    //Water
                }
                else if (b == 79)
                {
                    color = new[] {255, 167, 156};
                    //ICE
                }
            }

            return color;
        }

        public static int[] getHieghtToColor2(int height, int max = 180)
        {
            int[] color = new int[3] {0, 0, 0};
            int[] Red = new int[] {255, 0, 0};
            int[] Green = new int[] {0, 255, 0};
            int[] Blue = new int[] {0, 0, 255};
            float v = Math.Min(height, max) / (float) max;
            float nv = Math.Abs(v - 1);
            color[0] = (int) (255 * nv);
            color[1] = (int) (255 * v);

            return color;
        }

        public static int[] getHieghtToColor3(int height)
        {
            int[] color = new int[3] {0, 0, 0};
            color[0] = height;

            return color;
        }

        // private static Bitmap image = new Bitmap(MASTERX * 16, MASTERZ * 16);
        private static Bitmap image = new Bitmap(MASTERX , MASTERZ);


        static void PlotPixel(int x, int y, byte redValue,
            byte greenValue, byte blueValue)
        {
            image.SetPixel(x, y, Color.FromArgb(255, redValue, greenValue, blueValue));

            // int offset = ((256*4)*y)+(x*4);
            // _imageBuffer[offset] = blueValue;
            // _imageBuffer[offset+1] = greenValue;
            // _imageBuffer[offset+2] = redValue;
            // // Fixed alpha value (No transparency)
            // _imageBuffer[offset+3] = 255;
        }
        //
        // public static void start()
        // {
        //     new BiomeManager();
        //     Stopwatch s1 = new Stopwatch();
        //     Stopwatch s2 = new Stopwatch();
        //     Stopwatch s3 = new Stopwatch();
        //     s3.Start();
        //     var tg = new OpenExperimentalWorldProvider(123123);
        //     var ccl = new List<ChunkColumn>();
        //     for (var ax = 0; ax < MASTERX; ++ax)
        //     {
        //         s1.Reset();
        //         s1.Start();
        //         for (var az = 0; az < MASTERZ; ++az)
        //         {
        //             s2.Reset();
        //             s2.Start();
        //             var c = new ChunkColumn();
        //             c.X = ax;
        //             c.Z = az;
        //
        //             //CALCULATE RAIN
        //             var rainnoise = new FastNoise(123123);
        //             rainnoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
        //             rainnoise.SetFrequency(.015f);
        //             rainnoise.SetFractalType(FastNoise.FractalType.FBM);
        //             rainnoise.SetFractalOctaves(1);
        //             rainnoise.SetFractalLacunarity(.25f);
        //             rainnoise.SetFractalGain(1);
        //             //CALCULATE TEMP
        //             var tempnoise = new FastNoise(123123 + 1);
        //             tempnoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
        //             tempnoise.SetFrequency(.015f);
        //             tempnoise.SetFractalType(FastNoise.FractalType.FBM);
        //             tempnoise.SetFractalOctaves(1);
        //             tempnoise.SetFractalLacunarity(.25f);
        //             tempnoise.SetFractalGain(1);
        //
        //
        //             float rain = rainnoise.GetNoise(c.X, c.Z) + 1;
        //             float temp = tempnoise.GetNoise(c.X, c.Z) + 1;
        //
        //
        //             tg.PopulateChunk(c, rain, temp);
        //             ccl.Add(c);
        //             s2.Stop();
        //             // Console.WriteLine($"1111111ZZZZZZZZ === WARNING IT TOOOKKKK {s2.Elapsed} FOR 1 CHUNK");
        //         }
        //
        //         s1.Stop();
        //         Console.WriteLine(
        //             $"1111111XXXXXXXXXXXXXXXXXXXXXXXXXXXX !!!! TOOOOOKKKK {s1.Elapsed} FOR {MASTERZ} CHUNKS FOR ROW {ax}");
        //     }
        //
        //     Console.WriteLine("Chunk Generation Done!");
        //     CalcuateHeightImage(ccl);
        //     s3.Stop();
        //     Console.WriteLine("ERROR YO THIS TOOK " + s3.Elapsed);
        // }
        //
    }
}