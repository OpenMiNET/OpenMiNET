using System;
using MiNET.Blocks;
using MiNET.Worlds;

namespace OpenAPI.World.Biomes
{
    public class BeachBiome : AdvancedBiome
    {
        public int waterlevel;
        public int SandHeight;


        public BeachBiome() : base("Beach", new BiomeQualifications(0, 2, 1, 1.75f, 0.5f, 0.25f
            , 10))
        {
            BiomeQualifications.baseheight = 83; //30
            waterlevel = 75;
            SandHeight = waterlevel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="yheight"></param>
        /// <param name="maxheight"></param>
        /// <param name="rxx"></param>
        /// <param name="rzz"></param>
        /// <param name="cc"></param>
        public override void SmoothVerticalColumn(int yheight, int maxheight, int rxx, int rzz, ChunkColumn cc)
        {
            var bid = cc.GetBlockId(rxx, yheight, rzz);
            if (bid == new Wood().Id || bid == new Log().Id) return;
            int sand = maxheight - 6;
            if (bid == new Water().Id || bid == new FlowingWater().Id) return;
            if (yheight < sand)
            {
                if (bid == 0) cc.SetBlock(rxx, yheight, rzz, new Stone());
            }
            // else if (cc.GetBlockId(rx, y, rz) == 0) break;
            else if (yheight < maxheight -3)
            {
                // if (x == 0 || z == map.GetLength(0) - 1 || z == 0 || z == map.GetLength(1) - 1)
                //     cc.SetBlock(rxx, y, rzz, new EmeraldBlock());
                /*else*/
                var r = new Random().Next(0, 10);
                if (r > 3)
                    cc.SetBlock(rxx, yheight, rzz, new Gravel());
                else
                    cc.SetBlock(rxx, yheight, rzz, new Sand());
                //350 350 + -
            }
            else if(yheight <= maxheight)
            {
                if (bid == 0 || bid == new Wood().Id ||bid == new Log().Id) return;
                cc.SetBlock(rxx, yheight, rzz, new Sand());
            }else
            {
                if (NotAllowedBlocks.Contains(bid))
                {
                    cc.SetBlock(rxx, yheight, rzz, new Air());
                }
            }
        }

        //TODO ADD CLAY
        
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

                var sh = BiomeQualifications.baseheight +
                         (int) GetNoise(c.X * 16 + x, c.Z * 16 + z, /*rth[2] / */.035f,
                             BiomeQualifications.heightvariation);
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
                            c.SetBlock(x, y, z, new Stone());
                        }
                        else if (y >= sh)
                        {
                            c.SetBlock(x, y, z, new Sand());
                        }
                        else
                        {
                            // var i = 0;
                            /*i = (new Random()).Next(0, 10);
                            if (i > 5) c.SetBlock(x, y, z, new Grass());
                            else*/
                            c.SetBlock(x, y, z, new Sand());
                            c.SetHeight(x, z, y);
                            break;
                        }
                    }
                    else
                    {
                        if (y <= sh - 3)
                        {
                            c.SetBlock(x, y, z, new Stone());
                            continue;
                        }

                        if (y <= sh)
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

                        c.SetHeight(x, z, (short) waterlevel); //y -1
                        break;
                    }
                }
            }
        }
    }
}