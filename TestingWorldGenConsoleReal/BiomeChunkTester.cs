using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Worlds;
using OpenAPI.World;
using OpenAPI.World.Biomes;

namespace TestingWorldGenConsoleReal
{
    public class BiomeChunkTester
    {
        public static int MASTERX = 256 * 2;
        public static int MASTERZ = 256 * 2;

        // private static readonly Bitmap image = new Bitmap(MASTERX * 16, MASTERZ * 16);
        private static readonly Bitmap image = new Bitmap(MASTERX, MASTERZ);

        private static void main()
        {
            Console.WriteLine("Hello World!");
            Console.WriteLine("Genertating 16 X 16 (256 Chunks) Chunk Radius to an Image!");
            new BiomeChunkTester().start();
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

        public static bool check(float minr, float maxr, float mint, float maxt, float minh, float maxh, float r,
            float t, float h)
        {
            return (minr < r && maxr > r && mint < t && maxt > t && minh < h && maxh > h);
        }

        public void start()
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

                   

                    //TODO FIX THIS IS OLD GET BIOME METHOD
                    var rth = getChunkRTH(c.X, c.Z);
                    var biome = BiomeManager.GetBiome2(rth);
                    
                    //CHEKC IF BOREDR CHUNK
                    //Top
                    var tb = BiomeManager.GetBiome2(getChunkRTH(c.X, c.Z+1));
                    var rb = BiomeManager.GetBiome2(getChunkRTH(c.X+1, c.Z));
                    var bb = BiomeManager.GetBiome2(getChunkRTH(c.X, c.Z-1));
                    var lb = BiomeManager.GetBiome2(getChunkRTH(c.X-1, c.Z));
                    if (tb.LocalID != biome.LocalID)
                    {
                        biome.BorderChunk = true;
                        biome.BorderBiome = tb;
                        biome.BorderType = 1;
                    }else if(rb.LocalID != biome.LocalID)
                    {
                        biome.BorderChunk = true;
                        biome.BorderBiome = rb;
                        biome.BorderType = 1;
                    }else if(bb.LocalID != biome.LocalID)
                    {
                        biome.BorderChunk = true;
                        biome.BorderBiome = bb;
                        biome.BorderType = 1;
                    }else if(lb.LocalID != biome.LocalID)
                    {
                        biome.BorderChunk = true;
                        biome.BorderBiome = lb;
                        biome.BorderType = 1;
                    }
                    else
                    {
                        biome.BorderChunk = false;
                    }




                    var rain = rth[0];
                    var temp = rth[1];
                    var height = rth[2];

//CALCULATE BIOME'S COLOR
                    byte[] v = new byte[3];
                     if (check(0, 2, 0, .5f, 0, .5f, rain, temp, height))
                    {
                        //A-Snow Icy Chunk
                        // v = new byte[] {0, 208, 255};
                        var a = Color.Beige;
                        v = new[] {a.R, a.G, a.B};
                    }
                    else if (check(0, 2, 0, .5f, 1.25f, 2, rain, temp, height))
                    {
                        v = new byte[] {0, 153, 255};

                        var a = Color.Bisque;
                        v = new[] {a.R, a.G, a.B};
                        //A-Snow Mountian
                    }
                    else if (check(.5f, 1.25f, 0, .5f, .75f, 1.25f, rain, temp, height))
                    {
                        var a = Color.LightGreen;
                        v = new[] {a.R, a.G, a.B};
                        v = new byte[] {0, 255, 229};
                        //A-Snow Taiga / Forest
                    }
                    else if (check(0, 2, 0, .5f, 1, .5f, rain, temp, height))
                    {
                        v = new byte[] {56, 115, 138};

                        var a = Color.Fuchsia;
                        v = new[] {a.R, a.G, a.B};
                        //A-Snow Tundra
                    }
                    else if (check(0.25f, 1, 0.75f, 1.75f, 1.25f, 2, rain, temp, height))
                    {
                        v = new byte[] {4, 128, 0};

                        var a = Color.Coral;
                        v = new[] {a.R, a.G, a.B};
                        //B-Mountains
                    }
                    else if (check(0, .25f, .5f, .75f, 1.25f, 2, rain, temp, height))
                    {
                        v = new byte[] {133, 133, 133};

                        var a = Color.Gray;
                        v = new[] {a.R, a.G, a.B};
                        //B-Stone Mountains
                    }
                    else if (check(1, 2, 0.5f, 1.75f, 1.25f, 2, rain, temp, height))
                    {
                        v = new byte[] {0, 208, 255};

                        var a = Color.Khaki;
                        v = new[] {a.R, a.G, a.B};
                        //B-Forest Mountains
                    }
                    else if (check(0.5f, 1, 0.5f, 1.75f, 0.5f, 1.25f, rain, temp, height))
                    {
                        v = new byte[] {0, 208, 255};


                        var a = Color.ForestGreen;
                        v = new[] {a.R, a.G, a.B};
                        //B-Taiga/Forest
                    }
                    else if (biome.name.Equals(new WaterBiome().name))
                    {
                        var a = Color.Aqua;
                        v = new[] {a.R, a.G, a.B};
                        //B-Water
                    }else if (biome.name.Equals(new HighPlains().name))
                    {
                        var a = Color.GreenYellow;
                        v = new[] {a.R, a.G, a.B};
                        //B-Water
                    }
                    else if (check(0.5f, 1.5f, 0.5f, 1.75f, 0.5f, 1, rain, temp, height))
                    {
                        v = new byte[] {196, 255, 168};

                        var a = Color.Gold;
                        v = new[] {a.R, a.G, a.B};
                        //B-Plains
                    }
                    else if (check(0, 2, 1.75f, 2, .5f, 1, rain, temp, height))
                    {
                        v = new byte[] {255, 254, 168};

                        var a = Color.DarkGoldenrod;
                        v = new[] {a.R, a.G, a.B};
                        //C-Desert
                    }
                    else if (check(1.25f, 2, .5f, 1, .75f, 1.5f, rain, temp, height))
                    {
                        v = new byte[] {255, 254, 168};

                        var a = Color.Blue;
                        v = new[] {a.R, a.G, a.B};
                        //C-Desert
                    }
                    else
                     {
                         var cc = Color.Purple;
                         v[0] = cc.R;
                         v[1] = cc.G;
                         v[2] = cc.B;
                         // Console.WriteLine(
                         // $"ERRRRRRRRRRRRRROOOOOOOOOOOOOORRRRRRRRRRRR WTFFFFFFFFFFFFFFFFFFFFFFFF {rain} {temp} {height}");
                         // Console.WriteLine(
                         //     $"ERRRRRRRRRRRRRROOOOOOOOOOOOOORRRRRRRRRRRR WTFFFFFFFFFFFFFFFFFFFFFFFF {rain} {temp} {height}");
                         // Console.WriteLine(
                         //     $"ERRRRRRRRRRRRRROOOOOOOOOOOOOORRRRRRRRRRRR WTFFFFFFFFFFFFFFFFFFFFFFFF {rain} {temp} {height}");
                         // Console.WriteLine(
                         //     $"ERRRRRRRRRRRRRROOOOOOOOOOOOOORRRRRRRRRRRR WTFFFFFFFFFFFFFFFFFFFFFFFF {rain} {temp} {height}");
                         // Console.WriteLine(
                         //     $"ERRRRRRRRRRRRRROOOOOOOOOOOOOORRRRRRRRRRRR WTFFFFFFFFFFFFFFFFFFFFFFFF {rain} {temp} {height}");
                         // Console.WriteLine(
                         //     $"ERRRRRRRRRRRRRROOOOOOOOOOOOOORRRRRRRRRRRR WTFFFFFFFFFFFFFFFFFFFFFFFF {rain} {temp} {height}");
                         // Console.WriteLine(
                         //     $"ERRRRRRRRRRRRRROOOOOOOOOOOOOORRRRRRRRRRRR WTFFFFFFFFFFFFFFFFFFFFFFFF {rain} {temp} {height}");
                     }
                     if (biome.BorderChunk)
                     {
                         v[0] = 255;
                         v[1] = 0;
                         v[2] = 0;
                     }

                    PlotPixel(c.X, c.Z, (byte) v[0], (byte) v[1], (byte) v[2]);


                    // Console.WriteLine($"1111111ZZZZZZZZ === WARNING IT TOOOKKKK {s2.Elapsed} FOR 1 CHUNK");
                }

                s1.Stop();
                // Console.WriteLine(
                //     $"3222XXXXXXXXXXXXXXXXXXXXXXXXXXXX !!!! TOOOOOKKKK {s1.Elapsed} FOR {MASTERZ} CHUNKS FOR ROW {ax}");
            }

            Console.WriteLine("Chunk Generation Done!");

            image.Save("D:\\WORLDGEN.png");
            s3.Stop();
            Console.WriteLine("ERROR YO THIS TOOK " + s3.Elapsed);
        }

        private static float[] getChunkRTH(in int cX, in int cZ)
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
            
            float rain = rainnoise.GetNoise(cX, cZ) + 1;
            float temp = tempnoise.GetNoise(cX, cZ) + 1;
            var height =  GetNoise(cX, cZ, 0.005f,
                2);;
            return new[] {rain, temp, height};

        }

        private static readonly OpenSimplexNoise OpenNoise = new OpenSimplexNoise("a-seed".GetHashCode());


        public static float GetNoise(int x, int z, float scale, int max)
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
    }
    
}