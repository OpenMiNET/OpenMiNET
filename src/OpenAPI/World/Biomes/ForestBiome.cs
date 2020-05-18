using System;
using System.Threading.Tasks;
using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Worlds;

namespace OpenAPI.World
{
    public class ForestBiome : AdvancedBiome
    {
        public ForestBiome() : base("ForestBiome", new BiomeQualifications(0.5f, 1, 0.5f, 1.75f, 0.5f, 1.25f
            , 30))
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="openExperimentalWorldProvider"></param>
        /// <param name="chunk"></param>
        /// <param name="rth"></param>
        /// <returns></returns>
        public override ChunkColumn  GenerateSurfaceItems(
            OpenExperimentalWorldProvider o, ChunkColumn chunk, float[] rth)
        {
            int cx = chunk.X;
            int cz = chunk.Z;
            int rx =(new Random()).Next(0,15);
            int rz =(new Random()).Next(0,15);
            int x = cx * 16 + rx;
            int z = cz * 16 + rz;
            Console.WriteLine($"TRYINGGGG FORRR TEEEEEEEEEEEEEEEEEEEEEEEEEEEE {cx} {cz}|| {x} {z}");
            int fy = chunk.GetHeight(rx, rz);
            var c = chunk;
            var ffy = 0;
            int retries = 1;
            while (true)
            {
                Console.WriteLine("TRYINGGGG FORRR TEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
                var r = RNDM.Next(0, 256);
                if (r < 25*3)
                {
                    var w = RNDM.Next(3, 5);
                    var h = RNDM.Next(6, 14);
                    var v = h - w;
                    var vv = 0;
                    ffy = fy + h;
                    for (var hh = 1; hh < h; hh++)
                    {
                        c.SetBlock(rx, fy + hh, rz, new Wood
                        {
                            WoodType = "birch"
                        });
                        //Bottom Half Leaves
                        if (hh > v /*&& v < (int)Math.Ceiling(w/3f)*/)
                        {
                            vv++;
                            var ww = vv;
                            for (var teir = 1; teir <= ww; teir++)
                            for (var teirn = 1; teirn <= teir; teirn++)
                            for (var xx = 0; xx <= teirn; xx++)
                            for (var zz = 0; zz <= teirn; zz++)
                            {
                                if (xx == 0 && zz == 0) continue;
                                // c.SetBlock(x , fy + hh, z+zz, new Leaves());
                                Console.WriteLine($"SETTING LEVES WITH {x} {z} || {xx} {zz} || {x+xx} {z+zz} || {teir}");
                                // o.Level.SetBlockFromCache(new Leaves
                                // {
                                //     OldLeafType = "jungle",
                                //     Coordinates = new BlockCoordinates(x + xx, fy + hh, z + zz)
                                // });  o.Level.SetBlockFromCache(new Leaves
                                // {
                                //     OldLeafType = "jungle",
                                //     Coordinates = new BlockCoordinates(x + xx, fy + hh, z - zz)
                                // });  o.Level.SetBlockFromCache(new Leaves
                                // {
                                //     OldLeafType = "jungle",
                                //     Coordinates = new BlockCoordinates(x - xx, fy + hh, z + zz)
                                // });  o.Level.SetBlockFromCache(new Leaves
                                // {
                                //     OldLeafType = "jungle",
                                //     Coordinates = new BlockCoordinates(x - xx, fy + hh, z - zz)
                                // });
                                if(rx+xx > 0 && rx+xx < 16 &&rz+zz > 0 && rz+zz < 16)
                                c.SetBlock(rx + xx, fy + hh, rz + zz, new Leaves
                                {
                                    OldLeafType = "jungle"
                                });
                                else
                                    o.Level.SetBlockFromCache(new Leaves
                                    {
                                        OldLeafType = "jungle",
                                        Coordinates = new BlockCoordinates(x + xx, fy + hh, z + zz)
                                    }); 
                                
                                if(rx+xx > 0 && rx+xx < 16 &&rz-zz > 0 && rz-zz < 16)
                                c.SetBlock(rx + xx, fy + hh, rz - zz, new Leaves
                                {
                                    OldLeafType = "jungle"
                                });
                                
                                else
                                    o.Level.SetBlockFromCache(new Leaves
                                    {
                                        OldLeafType = "jungle",
                                        Coordinates = new BlockCoordinates(x + xx, fy + hh, z - zz)
                                    }); 
                                if(rx-xx > 0 && rx-xx < 16 &&rz+zz > 0 && rz+zz < 16)
                                c.SetBlock(rx - xx, fy + hh, rz + zz, new Leaves
                                {
                                    OldLeafType = "jungle"
                                });
                                else
                                    o.Level.SetBlockFromCache(new Leaves
                                    {
                                        OldLeafType = "jungle",
                                        Coordinates = new BlockCoordinates(x - xx, fy + hh, z + zz)
                                    }); 
                                if(rx-xx > 0 && rx-xx < 16 &&rz-zz > 0 && rz-zz < 16)
                                c.SetBlock(rx - xx, fy + hh, rz - zz, new Leaves
                                {
                                    OldLeafType = "jungle"
                                });
                                else
                                    o.Level.SetBlockFromCache(new Leaves
                                    {
                                        OldLeafType = "jungle",
                                        Coordinates = new BlockCoordinates(x - xx, fy + hh, z - zz)
                                    }); 
                            }
                        }
                    }

                    //Top Leaves
                    // for (var vvv = vv; vvv > 0; vvv--)
                    // {
                    //     for (var teir = vvv; teir > 0; teir--)
                    //     for (var teirn = 1; teirn <= teir; teirn++)
                    //     for (var xx = 0; xx <= teirn; xx++)
                    //     for (var zz = 0; zz <= teirn; zz++)
                    //     {
                    //         // if(xx == 0 && zz == 0)continue;
                    //
                    //         c.SetBlock(x + xx, ffy, z + zz, new Leaves
                    //         {
                    //             OldLeafType = "jungle"
                    //         });
                    //         c.SetBlock(x + xx, ffy, z - zz, new Leaves
                    //         {
                    //             OldLeafType = "jungle"
                    //         });
                    //         c.SetBlock(x - xx, ffy, z + zz, new Leaves
                    //         {
                    //             OldLeafType = "jungle"
                    //         });
                    //         c.SetBlock(x - xx, ffy, z - zz, new Leaves
                    //         {
                    //             OldLeafType = "jungle"
                    //         });
                    //     }
                    //
                    //     ffy++;
                    // }

                    for (var teir = 0; teir <= v; teir++)
                    {
                    }
                }
                else
                {
                    retries--;
                    if (retries == 0)
                    {
                        return c;
                    }
                }
            }

            return chunk;
        }

        public override void PopulateChunk(OpenExperimentalWorldProvider o,
            ChunkColumn c, float[] rth)
        {
            // int sh =

            for (var x = 0; x < 16; x++)
            for (var z = 0; z < 16; z++)
            {
                // float h = HeightNoise.GetNoise(c.X * 16 + x, c.Z * 16 + z)+1;
                // int sh= (int) Math.Floor(BiomeQualifications.baseheight + ((rth[2] )* BiomeQualifications.heightvariation))+(int)(HeightNoise.GetNoise(c.X * 16 + x, c.Z * 16 + z) * 10);
                // int sh= (int) Math.Floor(BiomeQualifications.baseheight + ((rth[2] )* BiomeQualifications.heightvariation))+(int)(GetNoise(c.X * 16 + x, c.Z * 16 + z,0.035f,10));
                // int sh = (int) (BiomeQualifications.baseheight +
                //                 (rth[2] * BiomeQualifications.heightvariation) +
                //                 (int) (GetNoise(c.X * 16 + x, c.Z * 16 + z, 0.035f, 5)));
                var sh = BiomeQualifications.baseheight + 12 +
                         (int) GetNoise(c.X * 16 + x, c.Z * 16 + z, /*rth[2] / */.035f,
                             BiomeQualifications.heightvariation / 5);
                // (int) (GetNoise(c.X * 16 + x, c.Z * 16 + z, 0.035f, 5)); //10
                // Console.WriteLine("FORRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRR >>>>>>>>>>>>>>> " + sh + " |||| " + rth[2]);

                // int sh = (int) (BiomeQualifications.baseheight +
                //                 (GetNoise(c.X * 16 + x, c.Z * 16 + z, 0.035f / 3,
                //                     BiomeQualifications.heightvariation)) +
                //                 (int) (GetNoise(c.X * 16 + x, c.Z * 16 + z, 0.035f, 5)));


                // int sh= (int) (BiomeQualifications.baseheight + GetNoise(c.X * 16 + x, c.Z * 16 + z,0.035f,(int)((rth[2] )* BiomeQualifications.heightvariation)+10));
                // int sh= (int) Math.Floor(BiomeQualifications.baseheight + ((rth[2])* BiomeQualifications.heightvariation));
                var fy = 0;
                for (var y = 0; y < 255; y++)
                {
                    if (y == 0)
                    {
                        c.SetBlock(x, y, z, new Bedrock());
                        continue;
                    }

                    if (y <= sh - 5)
                    {
                        c.SetBlock(x, y, z, new Stone());
                        continue;
                    }

                    if (y < sh)
                    {
                        var r = RNDM.Next(0, 3);
                        if (r == 0) c.SetBlock(x, y, z, new Stone());
                        if (r == 1) c.SetBlock(x, y, z, new Dirt());
                        if (r == 2) c.SetBlock(x, y, z, new Dirt());
                        if (r == 3) c.SetBlock(x, y, z, new Stone());
                        continue;
                    }

                    c.SetBlock(x, y, z, new GrassPath());


                    c.SetHeight(x, z, (short) y);
                    fy = y;
                    break;
                }

                if (RNDM.Next(0, 100) < 15) c.SetBlock(x, fy + 1, z, new Tallgrass());

                if (RNDM.Next(0, 300) < 3) c.SetBlock(x, fy + 1, z, new YellowFlower());

                if (RNDM.Next(0, 300) < 3) c.SetBlock(x, fy + 1, z, new RedFlower());

                if (RNDM.Next(0, 300) < 8)
                {
                    c.SetBlock(x, fy + 1, z, new DoublePlant());
                    c.SetBlock(x, fy + 2, z, new YellowFlower());
                }
            }
        }
    }
}