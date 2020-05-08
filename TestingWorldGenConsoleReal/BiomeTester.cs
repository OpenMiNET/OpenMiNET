using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using MiNET.Blocks;
using MiNET.Worlds;
using OpenAPI.World;

namespace TestingWorldGenConsoleReal
{
    public class BiomeTester
    {
        public static int MASTERX = 100*2;
        public static int MASTERZ = 100*2;

        // private static readonly Bitmap image = new Bitmap(MASTERX * 16, MASTERZ * 16);
        private static readonly Bitmap image = new Bitmap(MASTERX , MASTERZ );

        private static void main()
        {
            Console.WriteLine("Hello World!");
            Console.WriteLine("Genertating 16 X 16 (256 Chunks) Chunk Radius to an Image!");
            start();
        }


        public static void CalcuateHeightImage(List<ChunkColumn> ccl)
        {
            var max = 0;
            foreach (var ccc in ccl)
                for (var ax = 0; ax < 16; ++ax)
                for (var az = 0; az < 16; ++az)
                {
                    int i = ccc.GetHeight(ax, az);
                    if (i > max) max = i;
                }

            // var height = new int[256, 256];
            foreach (var cc in ccl)
                for (var ax = 0; ax < 16; ++ax)
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

            image.Save("D:\\WORLDGEN.png");
        }

        public static int[] getHieghtToColor(int height)
        {
            var color = new int[3] {0, 0, 0};
            int a;
            if (height < 50)
            {
                a = height / 50;
                color[0] = a * 101;
                color[1] = a * 61;
            }
            else if (height < 100)
            {
                a = height - 50 / 50;
                color[0] = a * 255;
                color[1] = a * 219;
            }
            else if (height < 150)
            {
                a = height - 100 / 50;
                color[0] = a * 140;
                color[1] = a * 255;
            }
            else if (height < 200)
            {
                a = height - 150 / 50;
                color[0] = a * 13;
                color[1] = a * 102;
            }
            else if (height < 250)
            {
                a = height - 50 / 50;
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
            var color = new int[3] {0, 0, 0};

            int a;
            int aa;
            if (height < 127)
            {
                a = height / 127;
                aa = a * -1 + 127;
                color[0] = aa * 255;
                color[2] = a * 50;
            }
            else
            {
                a = height - 127 / 127;
                aa = a * -1 + 127;
                color[0] = a * 50;
                color[2] = aa * 255;
            }

            return color;
        }

        public static int[] getHieghtToColor2b(Block cc, int height, int max = 180)
        {
            var color = new int[3] {0, 0, 0};
            int[] Red = {255, 0, 0};
            int[] Green = {0, 255, 0};
            int[] Blue = {0, 0, 255};
            var v = Math.Min(height, max) / (float) max;
            var nv = Math.Abs(v - 1);
            color[0] = (int) (255 * nv);
            color[1] = (int) (255 * v);
            if (v > 5f / 256)
            {
                var b = cc.Id;
                if (b == 223)
                    color = new[] {60, 50, 168};
                //GreenGlazedTerracotta
                else if (b == 152)
                    //RedstoneBlock
                    color = new[] {131, 50, 168};
                else if (b == 179)
                    //RedSandstone
                    color = new[] {169, 107, 50};
                else if (b == 9)
                    color = new[] {50, 151, 168};
                //Water
                else if (b == 79)
                    color = new[] {255, 167, 156};
                //ICE
            }

            return color;
        }

        public static int[] getHieghtToColor2(int height, int max = 180)
        {
            var color = new int[3] {0, 0, 0};
            int[] Red = {255, 0, 0};
            int[] Green = {0, 255, 0};
            int[] Blue = {0, 0, 255};
            var v = Math.Min(height, max) / (float) max;
            var nv = Math.Abs(v - 1);
            color[0] = (int) (255 * nv);
            color[1] = (int) (255 * v);

            return color;
        }

        public static int[] getHieghtToColor3(int height)
        {
            var color = new int[3] {0, 0, 0};
            color[0] = height;

            return color;
        }


        private static void PlotPixel(int x, int y, byte redValue,
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

        public static bool check(float minr, float maxr, float mint, float maxt, float minh, float maxh, float r, float t,float h)
        {
            return (minr < r && maxr > r && mint < t && maxt > t && minh < h && maxh > h);
        }
        
        public static void start()
        {
            new BiomeManager();
            var s1 = new Stopwatch();
            var s2 = new Stopwatch();
            var s3 = new Stopwatch();
            s3.Start();
            var tg = new OpenExperimentalWorldProvider(123123);
            var ccl = new List<ChunkColumn>();
            for (var ax = 0; ax < MASTERX; ++ax)
            {
                s1.Reset();
                s1.Start();
                for (var az = 0; az < MASTERZ; ++az)
                {
                    s2.Reset();
                    s2.Start();
                    var c = new ChunkColumn();
                    c.X = ax;
                    c.Z = az;

                    //CALCULATE RAIN
                    var rainnoise = new FastNoise(123123);
                    rainnoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
                    rainnoise.SetFrequency(.015f);
                    rainnoise.SetFractalType(FastNoise.FractalType.FBM);
                    rainnoise.SetFractalOctaves(1);
                    rainnoise.SetFractalLacunarity(.25f);
                    rainnoise.SetFractalGain(1);
                    //CALCULATE TEMP
                    var tempnoise = new FastNoise(123123 + 1);
                    tempnoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
                    tempnoise.SetFrequency(.015f);
                    tempnoise.SetFractalType(FastNoise.FractalType.FBM);
                    tempnoise.SetFractalOctaves(1);
                    tempnoise.SetFractalLacunarity(.25f);
                    tempnoise.SetFractalGain(1);
                    //CALCULATE HEIGHT
                    var heightnoise = new FastNoise(123123 + 2);
                    heightnoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
                    heightnoise.SetFrequency(.015f);
                    heightnoise.SetFractalType(FastNoise.FractalType.FBM);
                    heightnoise.SetFractalOctaves(1);
                    heightnoise.SetFractalLacunarity(.25f);
                    heightnoise.SetFractalGain(1);


                    var rain = rainnoise.GetNoise(c.X, c.Z) + 1;
                    var temp = tempnoise.GetNoise(c.X, c.Z) + 1;
                    var height = heightnoise.GetNoise(c.X, c.Z) + 1;

//CALCULATE BIOME'S COLOR
                    byte[] v = new byte[3];
                    if (check(0,2,0,.5f,0,.5f,rain,temp,height))
                    {
                        //A-Snow Icy Chunk
                        // v = new byte[] {0, 208, 255};
                        var a = Color.Beige;
                        v = new [] {a.R,a.G,a.B};
                    }
                    else if (check(0,2,0,.5f,1.25f,2,rain,temp,height))
                    {
                        v = new byte[] {0, 153, 255};
                        
                        var a = Color.Bisque;
                        v = new [] {a.R,a.G,a.B};
                        //A-Snow Mountian
                    }
                    else if (check(.5f,1.25f,0,.5f,.75f,1.25f,rain,temp,height))
                    {
                        
                        var a = Color.LightGreen;
                        v = new [] {a.R,a.G,a.B};
                        v = new byte[] {0, 255, 229};
                        //A-Snow Taiga / Forest
                    }
                    else if (check(0,2,0,.5f,1,.5f,rain,temp,height))
                    {
                        v = new byte[] {56, 115, 138};
                        
                        var a = Color.Fuchsia;
                        v = new [] {a.R,a.G,a.B};
                        //A-Snow Tundra
                    }
                    else if (check(0.25f,1,0.75f,1.75f,1.25f,2,rain,temp,height))
                    {
                        v = new byte[] {4, 128, 0};
                     
                        var a = Color.Coral;
                        v = new [] {a.R,a.G,a.B};
                        //B-Mountains
                    }
                    else if (check(0,.25f,.5f,.75f,1.25f,2,rain,temp,height))
                    {
                        
                        v = new byte[] {133, 133, 133};
                        
                        var a = Color.Gray;
                        v = new [] {a.R,a.G,a.B};
                        //B-Stone Mountains
                    }
                    else if (check(1,2,0.5f,1.75f,1.25f,2,rain,temp,height))
                    {
                        
                        v = new byte[] {0, 208, 255};
                        
                        var a = Color.Khaki;
                        v = new [] {a.R,a.G,a.B};
                        //B-Forest Mountains
                    }
                    else if (check(0.5f,1,0.5f,1.75f,0.5f,1.25f,rain,temp,height))
                    {
                        v = new byte[] {0, 208, 255};
                        
                        
                        var a = Color.ForestGreen;
                        v = new [] {a.R,a.G,a.B};
                        //B-Taiga/Forest
                    }
                    else if (check(0,2,1,1.75f,0,.5f,rain,temp,height))
                    {
                        var a = Color.Aqua;
                        v = new [] {a.R,a.G,a.B};
                        //B-Water
                    }
                    else if (check(0.5f,1.5f,0.5f,1.75f,0.5f,1,rain,temp,height))
                    {
                        v = new byte[] {196, 255, 168};
                     
                        var a = Color.Gold;
                        v = new [] {a.R,a.G,a.B};
                        //B-Plains
                    }
                    else if (check(0,2,1.75f,2,.5f,1,rain,temp,height))
                    {
                        v = new byte[] {255, 254, 168};
                        
                        var a = Color.DarkGoldenrod;
                        v = new [] {a.R,a.G,a.B};
                        //C-Desert
                    }
                    else if (check(1.25f,2,.5f,1,.75f,1.5f,rain,temp,height))
                    {
                        v = new byte[] {255, 254, 168};
                        
                        var a = Color.Blue;
                        v = new [] {a.R,a.G,a.B};
                        //C-Desert
                    }
                    else
                    {
                        v = new byte[] {0, 0, 0};
                        Console.WriteLine($"ERRRRRRRRRRRRRROOOOOOOOOOOOOORRRRRRRRRRRR WTFFFFFFFFFFFFFFFFFFFFFFFF {rain} {temp} {height}");
                        Console.WriteLine($"ERRRRRRRRRRRRRROOOOOOOOOOOOOORRRRRRRRRRRR WTFFFFFFFFFFFFFFFFFFFFFFFF {rain} {temp} {height}");
                        Console.WriteLine($"ERRRRRRRRRRRRRROOOOOOOOOOOOOORRRRRRRRRRRR WTFFFFFFFFFFFFFFFFFFFFFFFF {rain} {temp} {height}");
                        Console.WriteLine($"ERRRRRRRRRRRRRROOOOOOOOOOOOOORRRRRRRRRRRR WTFFFFFFFFFFFFFFFFFFFFFFFF {rain} {temp} {height}");
                        Console.WriteLine($"ERRRRRRRRRRRRRROOOOOOOOOOOOOORRRRRRRRRRRR WTFFFFFFFFFFFFFFFFFFFFFFFF {rain} {temp} {height}");
                        Console.WriteLine($"ERRRRRRRRRRRRRROOOOOOOOOOOOOORRRRRRRRRRRR WTFFFFFFFFFFFFFFFFFFFFFFFF {rain} {temp} {height}");
                        Console.WriteLine($"ERRRRRRRRRRRRRROOOOOOOOOOOOOORRRRRRRRRRRR WTFFFFFFFFFFFFFFFFFFFFFFFF {rain} {temp} {height}");
                        
                    }

                    PlotPixel(c.X, c.Z, (byte) v[0], (byte) v[1], (byte) v[2]);


                    // Console.WriteLine($"1111111ZZZZZZZZ === WARNING IT TOOOKKKK {s2.Elapsed} FOR 1 CHUNK");
                }

                s1.Stop();
                Console.WriteLine(
                    $"3222XXXXXXXXXXXXXXXXXXXXXXXXXXXX !!!! TOOOOOKKKK {s1.Elapsed} FOR {MASTERZ} CHUNKS FOR ROW {ax}");
            }

            Console.WriteLine("Chunk Generation Done!");
            
            image.Save("D:\\WORLDGEN.png");
            s3.Stop();
            Console.WriteLine("ERROR YO THIS TOOK " + s3.Elapsed);
        }
    }
}