using System;
using MiNET.Blocks;
using MiNET.Worlds;

namespace OpenAPI.World
{
    public class ForestBiome : AdvancedBiome
    {
        public ForestBiome() : base("ForestBiome", new BiomeQualifications(0.5f, 1, 0.5f, 1.75f, 0.5f, 1.25f
            , 30))
        {
        }

        public override void PopulateChunk(ChunkColumn c, float[] rth)
        {
            for (var x = 0; x < 16; x++)
            for (var z = 0; z < 16; z++)
            {
                c.SetBlock(x, 0, z, new Bedrock());
                c.SetBlock(x, 1, z, new Wood());
                c.SetHeight(x, z, 1);
            }
        }
    }

    public class SnowyIcyChunk : AdvancedBiome
    {
        public SnowyIcyChunk() : base("SnowyIcyChunk", new BiomeQualifications(0, 2, 0, .5f, 0, .5f, 30))
        {
        }

        public override void PopulateChunk(ChunkColumn c, float[] rth)
        {
            for (var x = 0; x < 16; x++)
            for (var z = 0; z < 16; z++)
            {
                c.SetBlock(x, 0, z, new Bedrock());
                c.SetBlock(x, 1, z, new PackedIce());
                c.SetHeight(x, z, 1);
            }
        }
    }


    public class SnowForest : AdvancedBiome
    {
        public SnowForest() : base("SnowForest", new BiomeQualifications(.5f, 1.25f, 0, 0.5f, .75f, 1.25f, 30))
        {
        }

        public override void PopulateChunk(ChunkColumn c, float[] rth)
        {
            for (var x = 0; x < 16; x++)
            for (var z = 0; z < 16; z++)
            {
                c.SetBlock(x, 0, z, new Bedrock());
                c.SetBlock(x, 1, z, new Snow());
                if (new Random().Next(10) >= 9)
                {
                    c.SetBlock(x, 2, z, new Wood());
                    c.SetHeight(x, z, 2);
                }
                else
                {
                    c.SetHeight(x, z, 1);
                }
            }
        }
    }


    public class SnowTundra : AdvancedBiome
    {
        public SnowTundra() : base("SnowTundra", new BiomeQualifications(0, 2, 0, .5f, .5f, 1, 30))
        {
        }

        public override void PopulateChunk(ChunkColumn c, float[] rth)
        {
            for (var x = 0; x < 16; x++)
            for (var z = 0; z < 16; z++)
            {
                c.SetBlock(x, 0, z, new Bedrock());
                c.SetBlock(x, 1, z, new BlueIce());
                c.SetHeight(x, z, 1);
            }
        }
    }


    public class Mountains : AdvancedBiome
    {
        public Mountains() : base("Mountains", new BiomeQualifications(.25f, 1, .75f, 1.75f, 1.25f, 2, 30))
        {
        }

        public override void PopulateChunk(ChunkColumn c, float[] rth)
        {
            for (var x = 0; x < 16; x++)
            for (var z = 0; z < 16; z++)
            {
                c.SetBlock(x, 0, z, new Bedrock());
                c.SetBlock(x, 1, z, new Stonebrick());
                c.SetHeight(x, z, 1);
            }
        }
    }


    public class ForestMountain : AdvancedBiome
    {
        public ForestMountain() : base("ForestMountain", new BiomeQualifications(1, 2, .5f, 1.75f, 1.25f, 2, 30))
        {
        }

        public override void PopulateChunk(ChunkColumn c, float[] rth)
        {
            for (var x = 0; x < 16; x++)
            for (var z = 0; z < 16; z++)
            {
                c.SetBlock(x, 0, z, new Bedrock());
                var w = new Wool();
                w.Color = "green";
                c.SetBlock(x, 1, z, w);
                c.SetHeight(x, z, 1);
            }
        }
    }


    public class WaterBiome : AdvancedBiome
    {
        public WaterBiome() : base("Water", new BiomeQualifications(0, 2, 1, 1.75f, 0, 0.5f
            , 30))
        {
        }

        public override void PopulateChunk(ChunkColumn c, float[] rth)
        {
            for (var x = 0; x < 16; x++)
            for (var z = 0; z < 16; z++)
            {
                c.SetBlock(x, 0, z, new Bedrock());
                c.SetBlock(x, 1, z, new MiNET.Blocks.Water());
                c.SetHeight(x, z, 1);
            }
        }
    }


    public class Plains : AdvancedBiome
    {
        public Plains() : base("Plains", new BiomeQualifications(0.5f, 1.5f, 0.5f, 1.75f, 0.5f, 1f, 30))
        {
        }

        public override void PopulateChunk(ChunkColumn c, float[] rth)
        {
            for (var x = 0; x < 16; x++)
            for (var z = 0; z < 16; z++)
            {
                c.SetBlock(x, 0, z, new Bedrock());
                c.SetBlock(x, 1, z, new Clay());
                c.SetHeight(x, z, 1);
            }
        }
    }


    public class Desert : AdvancedBiome
    {
        public Desert() : base("Desert", new BiomeQualifications(0, 2, 1.75f, 2, 0.5f, 1
            , 30))
        {
        }

        public override void PopulateChunk(ChunkColumn c, float[] rth)
        {
            for (var x = 0; x < 16; x++)
            for (var z = 0; z < 16; z++)
            {
                c.SetBlock(x, 0, z, new Bedrock());
                c.SetBlock(x, 1, z, new RedSandstoneStairs());
                c.SetHeight(x, z, 1);
            }
        }
    }


    public class TropicalRainForest : AdvancedBiome
    {
        public TropicalRainForest() : base("TropicalRainForest", new BiomeQualifications(1.25f, 2, 0, 0.5f, 0.5f, 1.5f
            , 30))
        {
        }

        public override void PopulateChunk(ChunkColumn c, float[] rth)
        {
            for (var x = 0; x < 16; x++)
            for (var z = 0; z < 16; z++)
            {
                c.SetBlock(x, 0, z, new Bedrock());
                c.SetBlock(x, 1, z, new Leaves()
                {
                    OldLeafType = "jungle"
                });
                c.SetHeight(x, z, 1);
            }
        }
    }


    public class TropicalSeasonalForest : AdvancedBiome
    {
        public TropicalSeasonalForest() : base("TropicalSeasonalForest", new BiomeQualifications(0.5f, 1.25f, 0f, 0.5f,
            0.5f, 1.5f
            , 30))
        {
        }

        public override void PopulateChunk(ChunkColumn c, float[] rth)
        {
            for (var x = 0; x < 16; x++)
            for (var z = 0; z < 16; z++)
            {
                c.SetBlock(x, 0, z, new Bedrock());
                c.SetBlock(x, 1, z, new MiNET.Blocks.Water());
                c.SetHeight(x, z, 1);
            }
        }
    }
}