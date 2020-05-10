using System;
using MiNET.Blocks;
using MiNET.Worlds;

namespace OpenAPI.World.Biomes
{
    public class TropicalRainForest : AdvancedBiome
    {
        public TropicalRainForest() : base("TropicalRainForest", new BiomeQualifications(1.25f, 2, 0, 0.5f, 0.5f,
            1.5f
            , 30))
        {
        }

        public override void PopulateChunk(OpenExperimentalWorldProvider openExperimentalWorldProvider,
            ChunkColumn c, float[] rth)
        {
            int sh =
                (int) Math.Floor(BiomeQualifications.baseheight +
                                 (rth[2] * BiomeQualifications.heightvariation) * 1.5f);
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

                c.SetBlock(x, y, z, new Leaves()
                {
                    OldLeafType = "jungle"
                });
                c.SetHeight(x, z, (short) y);
                break;
            }
        }
    }

}