using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using MiNET.Worlds;
using OpenAPI.World;

namespace TestingWorldGenConsoleReal
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Console.WriteLine("Genertating 16 X 16 (256 Chunks) Chunk Radius to an Image!");
            start();
        }

        public static int MASTERX = 32;
        public static int MASTERZ = 32;


        public static void CalcuateHeightImage(List<ChunkColumn> ccl)
        {
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
                        // height[fx, fz] = mh;
                        byte r = 0;
                        byte g = 0;
                        byte b = 0;
                        var a = 0;
                        Console.WriteLine(mh + "<<<<<<<<<<");
                        if (mh < 100)
                        {
                            b = (byte) mh;
                        }
                        else if ( /*100 < mh && */ mh < 150)
                        {
                            a = (mh - 100) / 150 * 255;
                            g = (byte) mh;
                            b = (byte) a;
                        }
                        else
                        {
                            a = (mh - 150) / 255;
                            g = (byte) mh;
                            r = (byte) a;
                        }

                        Console.WriteLine(mh + $"<<<<<<<<<< {r}||{g}||{b}|||||||||||||||{a}");
                        // PlotPixel(fx, fz, r, g, b);
                        var v = getHieghtToColor3(mh);
                        PlotPixel(fx, fz, (byte) v[0], (byte) v[1], (byte) v[2]);
                        if (az == 0 || ax == 0) PlotPixel(fx, fz, 255, 255, 255);
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

        public static int[] getHieghtToColor2(int height)
        {
            int[] color = new int[3] {0, 0, 0};
            int a;
            int aa;
            if (height < 127)
            {
                a = ((height / 127));
                aa = (a*-1)+127;
                color[0] = aa * 255;
                color[2] = a * 50;
            }
            else
            {
                a = ((height-127 / 127));
                aa = (a*-1)+127;
                color[0] = a*50;
                color[2] = aa*255;
            }

            return color;
        }
        public static int[] getHieghtToColor3(int height)
        {
            int[] color = new int[3] {0, 0, 0};
                color[0] = height;

            return color;
        }

        private static Bitmap image = new Bitmap(MASTERX * 16, MASTERZ * 16);


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

        public static void start()
        {
            var tg = new TestGenerator(Dimension.Overworld);
            var ccl = new List<ChunkColumn>();
            for (var ax = 0; ax < MASTERX; ++ax)
            for (var az = 0; az < MASTERZ; ++az)
            {
                var c = new ChunkColumn();
                c.X = ax;
                c.Z = az;
                tg.PopulateChunk(c);
                ccl.Add(c);
            }

            Console.WriteLine("Chunk Generation Done!");
            CalcuateHeightImage(ccl);
        }
    }
}