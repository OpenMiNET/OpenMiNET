using System;
using MiNET.Blocks;
using MiNET.Worlds;

namespace OpenAPI.World
{
        public class WaterBiome : AdvancedBiome
    {
        public WaterBiome() : base("Water", new BiomeQualifications(0, 2, 1, 1.75f, 0, 0.5f
            , 55))
        {
            BiomeQualifications.baseheight = 30;
        }

        int waterlevel = 75;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="openExperimentalWorldProvider"></param>
        /// <param name="c"></param>
        /// <param name="rth"></param>
        // public new void SmoothChunk(OpenExperimentalWorldProvider openExperimentalWorldProvider, ChunkColumn c,
        //     float[] rth)
        // {
        //     int maxadjust = 20;
        //     ChunkColumn[] cc = new ChunkColumn[8];
        //     int a = 0;
        //     cc[0] = openExperimentalWorldProvider.GenerateChunkColumn(new ChunkCoordinates()
        //     {
        //         X = c.X - 1,
        //         Z = c.Z + 1
        //     }, true);
        //     cc[1] = openExperimentalWorldProvider.GenerateChunkColumn(new ChunkCoordinates()
        //     {
        //         X = c.X,
        //         Z = c.Z + 1
        //     }, true);
        //     cc[2] = openExperimentalWorldProvider.GenerateChunkColumn(new ChunkCoordinates()
        //     {
        //         X = c.X + 1,
        //         Z = c.Z + 1
        //     }, true);
        //     cc[3] = openExperimentalWorldProvider.GenerateChunkColumn(new ChunkCoordinates()
        //     {
        //         X = c.X - 1,
        //         Z = c.Z
        //     }, true);
        //     cc[4] = openExperimentalWorldProvider.GenerateChunkColumn(new ChunkCoordinates()
        //     {
        //         X = c.X + 1,
        //         Z = c.Z
        //     }, true);
        //     cc[5] = openExperimentalWorldProvider.GenerateChunkColumn(new ChunkCoordinates()
        //     {
        //         X = c.X - 1,
        //         Z = c.Z - 1
        //     }, true);
        //     cc[6] = openExperimentalWorldProvider.GenerateChunkColumn(new ChunkCoordinates()
        //     {
        //         X = c.X,
        //         Z = c.Z - 1
        //     }, true);
        //     cc[7] = openExperimentalWorldProvider.GenerateChunkColumn(new ChunkCoordinates()
        //     {
        //         X = c.X - 1,
        //         Z = c.Z - 1
        //     }, true);
        //     int ah = getAverageHeight(c);
        //     int avgh = 0;
        //     ChunkColumn workingchunk;
        //     workingchunk = cc[1];
        //     int[] directon = new int[2];
        //     if (workingchunk != null)
        //     {
        //         directon = new[] {-1, 1};
        //         var wrth = openExperimentalWorldProvider.getChunkRTH(workingchunk);
        //         var wb = BiomeManager.GetBiome(wrth);
        //         //TOP = 1
        //
        //         for (var x = 0; x < 16; x++)
        //         for (var z = 0; z < 16; z++)
        //         {
        //             // float h = HeightNoise.GetNoise(c.X * 16 + x, c.Z * 16 + z)+1;
        //             // int sh= (int) Math.Floor(BiomeQualifications.baseheight + ((rth[2] )* BiomeQualifications.heightvariation))+(int)(HeightNoise.GetNoise(c.X * 16 + x, c.Z * 16 + z) * 10);
        //             // int sh= (int) Math.Floor(BiomeQualifications.baseheight + ((rth[2] )* BiomeQualifications.heightvariation))+(int)(GetNoise(c.X * 16 + x, c.Z * 16 + z,0.035f,10));
        //             int sh = (int) (BiomeQualifications.baseheight +
        //                             (rth[2] * BiomeQualifications.heightvariation) +
        //                             (int) (GetNoise(c.X * 16 + x, c.Z * 16 + z, 0.035f, 5)));
        //             // int sh= (int) (BiomeQualifications.baseheight + GetNoise(c.X * 16 + x, c.Z * 16 + z,0.035f,(int)((rth[2] )* BiomeQualifications.heightvariation)+10));
        //             // int sh= (int) Math.Floor(BiomeQualifications.baseheight + ((rth[2])* BiomeQualifications.heightvariation));
        //             int fy = 0;
        //             for (var y = 0; y < 255; y++)
        //             {
        //                 if (y == 0)
        //                 {
        //                     c.SetBlock(x, y, z, new Bedrock());
        //                     continue;
        //                 }
        //
        //                 if (y <= sh - 5)
        //                 {
        //                     c.SetBlock(x, y, z, new Stone());
        //                     continue;
        //                 }
        //
        //                 if (y < sh)
        //                 {
        //                     int r = (RNDM).Next(0, 3);
        //                     if (r == 0) c.SetBlock(x, y, z, new Stone());
        //                     if (r == 1) c.SetBlock(x, y, z, new Dirt());
        //                     if (r == 2) c.SetBlock(x, y, z, new Dirt());
        //                     if (r == 3) c.SetBlock(x, y, z, new Stone());
        //                     continue;
        //                 }
        //
        //                 c.SetBlock(x, y, z, new Grass());
        //                 if (RNDM.Next(0, 100) < 15)
        //                 {
        //                 }
        //
        //                 c.SetHeight(x, z, (short) y);
        //                 fy = y;
        //                 break;
        //             }
        //
        //
        //             avgh = getAverageHeight(workingchunk);
        //             bool up = ah < avgh;
        //             if (up)
        //             {
        //             }
        //         }
        //     }
        // }
        public override void PopulateChunk(OpenExperimentalWorldProvider openExperimentalWorldProvider,
            ChunkColumn c,
            float[] rth)
        {
            // int stopheight =
            //     (int) Math.Floor(BiomeQualifications.baseheight + (rth[2] * BiomeQualifications.heightvariation));

            for (var x = 0; x < 16; x++)
            for (var z = 0; z < 16; z++)
            {
                // int sh = (int) (BiomeQualifications.baseheight +
                //                 (int) (GetNoise(c.X * 16 + x, c.Z * 16 + z, rth[2], BiomeQualifications.heightvariation)))+
                //                 (int) (GetNoise(c.X * 16 + x, c.Z * 16 + z, 0.035f, 5)); //10
                
                int sh = (int) (BiomeQualifications.baseheight +
                                (int) (GetNoise(c.X * 16 + x, c.Z * 16 + z, /*rth[2] / */.035f,
                                    BiomeQualifications.heightvariation)));
                // Console.WriteLine("WATTTTTTTTTEEEEEEEERRRRRRRRRRR >>>>>>>>>>>>>>> " + sh + " |||| " + rth[2]);
                for (short y = 0; y < 255; y++)
                {
                    if (y == 0)
                    {
                        c.SetBlock(x, y, z, new Bedrock());
                        continue;
                    }

                    if (sh >= waterlevel)
                    {
                        if (y <= sh - 3)
                        {
                            c.SetBlock(x,y,z,new Stone());
                        }else if (y >= sh)
                        {
                            c.SetBlock(x,y,z,new Sand());
                            
                        }
                        else
                        {
                            // var i = 0;
                            /*i = (new Random()).Next(0, 10);
                            if (i > 5) c.SetBlock(x, y, z, new Grass());
                            else*/
                            c.SetBlock(x, y, z, new Sand());
                            c.SetHeight(x,z,y);
                            break;
                        }
                    }
                    else
                    {
                        if (y <= sh - 3)
                        {
                            c.SetBlock(x,y,z,new Stone());
                            continue;
                        }
                        else if(y <= sh)
                        {
                            // var i = 0;
                            /*i = (new Random()).Next(0, 10);
                            if (i > 5) c.SetBlock(x, y, z, new Grass());
                            else*/
                            c.SetBlock(x, y, z, new Sand());
                            continue;
                        }
                        if (y <= waterlevel)
                        {
                            c.SetBlock(x, y, z, new FlowingWater());
                            continue;
                        }
                        
                        c.SetHeight(x, z, (short) waterlevel);//y -1
                        break;
                    }
                }
            }
        }
    }

}