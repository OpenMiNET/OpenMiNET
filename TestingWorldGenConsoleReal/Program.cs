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

        public static void CalcuateHeightImage(List<ChunkColumn> ccl)
        {
            var height = new int[256, 256];
            foreach (var cc in ccl)
            {
                for (var ax = 0; ax < 16; ++ax)
                {
                    for (var az = 0; az < 16; ++az)
                    {
                        var fx = cc.X * 16 + ax;
                        var fz = cc.Z * 16 + az;
                        var mh = 0;
                        mh = cc.GetHeight(ax,az);
                        height[fx, fz] = mh;
                        byte r = 0;
                        byte g = 0;
                        byte b = 0;
                        Console.WriteLine(mh+"<<<<<<<<<<");
                        if (mh < 100)
                        {
                            b = (byte) mh;
                        }else if (/*100 < mh && */ mh < 150)
                        {
                            var a = (mh - 100) * 255; 
                            g = (byte) mh;
                        }
                        else
                        {
                            g = (byte) mh;
                            r = (byte) mh;
                        }
                        Console.WriteLine(mh+$"<<<<<<<<<< {r}||{g}||{b}");
                        PlotPixel(fx,fz,r,g,b);
                        if(az == 0 || ax == 0)PlotPixel(fx,fz,255,255,255);
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

        private static Bitmap image = new Bitmap(256, 256);
        private static readonly byte[] _imageBuffer =
            new byte[262144];
 
        static void PlotPixel(int x, int y, byte redValue,
            byte greenValue, byte blueValue)
        {
            image.SetPixel(x,y,Color.FromArgb(255,redValue,greenValue,blueValue));
            
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
            for (var ax = 0; ax < 16; ++ax)
            for (var az = 0; az < 16; ++az)
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