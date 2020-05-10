using System;
using MiNET.Blocks;
using MiNET.Worlds;

namespace OpenAPI.World.Biomes
{
    public class Desert : AdvancedBiome
    {
        public Desert() : base("Desert", new BiomeQualifications(0, 2, 1.75f, 2, 0.5f, 1
            , 30))
        {
        }

        public override void PopulateChunk(OpenExperimentalWorldProvider openExperimentalWorldProvider,
            ChunkColumn c, float[] rth)
        {
            int sh =
                (int) Math.Floor(BiomeQualifications.baseheight + (rth[2] * BiomeQualifications.heightvariation));
            for (var x = 0; x < 16; x++)
            for (var z = 0; z < 16; z++)
            for (var y = 0; y < 255; y++)
            {
                if (y == 0)
                {
                    c.SetBlock(x, y, z, new Bedrock());
                    continue;
                }

                if (y <= sh)
                {
                    c.SetBlock(x, y, z, new Stone());
                    continue;
                }

                c.SetBlock(x, y, z, new RedSandstoneStairs());
                c.SetHeight(x, z, (short) y);
                break;
            }
        }
    }

}