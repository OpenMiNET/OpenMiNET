using MiNET.Blocks;
using MiNET.Worlds;

namespace OpenAPI.World.Biomes
{
        public class HighPlains : AdvancedBiome
    {
        public HighPlains() : base("HighPlains", new BiomeQualifications(0.5f, 1.5f, 0.5f, 1.75f, 0.75f, 1.5f, 30))
        {
        }

        public override void PopulateChunk(OpenExperimentalWorldProvider openExperimentalWorldProvider,
            ChunkColumn c, float[] rth)
        {
            // int sh =


            for (var x = 0; x < 16; x++)
            for (var z = 0; z < 16; z++)
            {
                // float h = HeightNoise.GetNoise(c.X * 16 + x, c.Z * 16 + z)+1;
                // int sh= (int) Math.Floor(BiomeQualifications.baseheight + ((rth[2] )* BiomeQualifications.heightvariation))+(int)(HeightNoise.GetNoise(c.X * 16 + x, c.Z * 16 + z) * 10);
                // int sh= (int) Math.Floor(BiomeQualifications.baseheight + ((rth[2] )* BiomeQualifications.heightvariation))+(int)(GetNoise(c.X * 16 + x, c.Z * 16 + z,0.035f,10));
                var sh = BiomeQualifications.baseheight +
                         (int) GetNoise(c.X * 16 + x, c.Z * 16 + z, /*rth[2] / */.035f,
                             BiomeQualifications.heightvariation);
                // int sh= (int) (BiomeQualifications.baseheight + GetNoise(c.X * 16 + x, c.Z * 16 + z,0.035f,(int)((rth[2] )* BiomeQualifications.heightvariation)+10));
                // int sh= (int) Math.Floor(BiomeQualifications.baseheight + ((rth[2])* BiomeQualifications.heightvariation));
                int fy = 0;
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
                        int r = (RNDM).Next(0, 3);
                        if (r == 0) c.SetBlock(x, y, z, new Stone());
                        if (r == 1) c.SetBlock(x, y, z, new Dirt());
                        if (r == 2) c.SetBlock(x, y, z, new Dirt());
                        if (r == 3) c.SetBlock(x, y, z, new Stone());
                        continue;
                    }

                    c.SetBlock(x, y, z, new Grass());
                    if (RNDM.Next(0, 100) < 15)
                    {
                    }

                    c.SetHeight(x, z, (short) y);
                    fy = y;
                    break;
                }

                if (RNDM.Next(0, 100) < 20)
                {
                    c.SetBlock(x, fy + 1, z, new Tallgrass());
                }

                if (RNDM.Next(0, 300) < 5)
                {
                    c.SetBlock(x, fy + 1, z, new YellowFlower());
                }

                if (RNDM.Next(0, 300) < 5)
                {
                    c.SetBlock(x, fy + 1, z, new RedFlower());
                }

                if (RNDM.Next(0, 300) < 5)
                {
                    c.SetBlock(x, fy + 1, z, new DoublePlant());
                    c.SetBlock(x, fy + 2, z, new YellowFlower());
                }

                //TREE
                int ffy = 0;
                if (x > 4 && x < 12 && z > 4 && z < 12)
                {
                    var r = RNDM.Next(0, 500);
                    if (r < 1)
                    {
                        int w = RNDM.Next(3, 5);
                        int h = RNDM.Next(6, 14);
                        int v = h - w;
                        int vv = 0;
                        ffy = fy + h;
                        for (int hh = 1; hh < h; hh++)
                        {
                            c.SetBlock(x, fy + hh, z, new Wood());
                            //Bottom Half Leaves
                            if (hh > v /*&& v < (int)Math.Ceiling(w/3f)*/)
                            {
                                vv++;
                                int ww = vv;
                                for (int teir = 1; teir <= ww; teir++)
                                {
                                    for (int teirn = 1; teirn <= teir; teirn++)
                                    {
                                        for (int xx = 0; xx <= teirn; xx++)
                                        {
                                            for (int zz = 0; zz <= teirn; zz++)
                                            {
                                                if (xx == 0 && zz == 0) continue;
                                                // c.SetBlock(x , fy + hh, z+zz, new Leaves());
                                                c.SetBlock(x + xx, fy + hh, z + zz, new Leaves());
                                                c.SetBlock(x + xx, fy + hh, z - zz, new Leaves());
                                                c.SetBlock(x - xx, fy + hh, z + zz, new Leaves());
                                                c.SetBlock(x - xx, fy + hh, z - zz, new Leaves());
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        //Top Leaves
                        for (int vvv = vv; vvv > 0; vvv--)
                        {
                            for (int teir = vvv; teir > 0; teir--)
                            {
                                for (int teirn = 1; teirn <= teir; teirn++)
                                {
                                    for (int xx = 0; xx <= teirn; xx++)
                                    {
                                        for (int zz = 0; zz <= teirn; zz++)
                                        {
                                            // if(xx == 0 && zz == 0)continue;
                                            // c.SetBlock(x , fy + hh, z+zz, new Leaves());
                                            c.SetBlock(x + xx, ffy, z + zz, new Leaves());
                                            c.SetBlock(x + xx, ffy, z - zz, new Leaves());
                                            c.SetBlock(x - xx, ffy, z + zz, new Leaves());
                                            c.SetBlock(x - xx, ffy, z - zz, new Leaves());
                                        }
                                    }
                                }
                            }

                            ffy++;
                        }

                        for (int teir = 0; teir <= v; teir++)
                        {
                        }
                    }
                }
            }
        }
    }

}