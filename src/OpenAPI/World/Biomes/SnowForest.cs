﻿using System;
using MiNET.Blocks;
using MiNET.Worlds;

namespace OpenAPI.World.Biomes
{
    public class SnowForest : AdvancedBiome
    {
        public SnowForest() : base("SnowForest", new BiomeQualifications(.5f, 1.25f, 0, 0.5f, .75f, 1.25f, 30))
        {
        }

        public override void PopulateChunk(OpenExperimentalWorldProvider openExperimentalWorldProvider, ChunkColumn c,
            float[] rth)
        {
            for (var x = 0; x < 16; x++)
            for (var z = 0; z < 16; z++){
                var sh = BiomeQualifications.baseheight +
                         (int) GetNoise(c.X * 16 + x, c.Z * 16 + z, /*rth[2] / */.035f,
                             BiomeQualifications.heightvariation);
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

                c.SetBlock(x, y, z, new Wool() {Color = "yellow"});
                c.SetHeight(x, z, (short) y);
                break;
            }
        }
    }

}
}