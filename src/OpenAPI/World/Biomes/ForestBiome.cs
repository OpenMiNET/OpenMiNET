using System;
using System.Threading.Tasks;
using MiNET.Blocks;
using MiNET.Worlds;
using OpenAPI.Player;

namespace OpenAPI.World
{
    public class ForestBiome : AdvancedBiome
    {
        public ForestBiome() : base("ForestBiome", new BiomeQualifications(0.5f, 1, 0.5f, 1.75f, 0.5f, 1.25f
            , 30))
        {
        }

        public override void PopulateChunk(OpenExperimentalWorldProvider openExperimentalWorldProvider, ChunkColumn c,
            float[] rth)
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

                c.SetBlock(x, y, z, new Wood());
                c.SetHeight(x, z, (short) y);
                break;
            }
        }
    }


    public class SnowyIcyChunk : AdvancedBiome
    {
        public SnowyIcyChunk() : base("SnowyIcyChunk", new BiomeQualifications(0, 2, 0, .5f, 0, .5f, 30))
        {
        }


        public override void PopulateChunk(OpenExperimentalWorldProvider openExperimentalWorldProvider, ChunkColumn c,
            float[] rth)
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

                c.SetBlock(x, y, z, new PackedIce());
                c.SetHeight(x, z, (short) y);
                break;
            }
        }
    }


    public class SnowForest : AdvancedBiome
    {
        public SnowForest() : base("SnowForest", new BiomeQualifications(.5f, 1.25f, 0, 0.5f, .75f, 1.25f, 30))
        {
        }

        public override void PopulateChunk(OpenExperimentalWorldProvider openExperimentalWorldProvider, ChunkColumn c,
            float[] rth)
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

                c.SetBlock(x, y, z, new Wool() {Color = "yellow"});
                c.SetHeight(x, z, (short) y);
                break;
            }
        }
    }


    public class SnowTundra : AdvancedBiome
    {
        public SnowTundra() : base("SnowTundra", new BiomeQualifications(0, 2, 0, .5f, .5f, 1, 30))
        {
        }

        public override void PopulateChunk(OpenExperimentalWorldProvider openExperimentalWorldProvider, ChunkColumn c,
            float[] rth)
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

                c.SetBlock(x, y, z, new BlueIce());
                c.SetHeight(x, z, (short) y);
                break;
            }
        }
    }


    public class Mountains : AdvancedBiome
    {
        public Mountains() : base("Mountains", new BiomeQualifications(.25f, 1, .75f, 1.75f, 1.25f, 2, 30))
        {
        }

        public override void PopulateChunk(OpenExperimentalWorldProvider openExperimentalWorldProvider, ChunkColumn c,
            float[] rth)
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

                c.SetBlock(x, y, z, new Stonebrick());
                c.SetHeight(x, z, (short) y);
                break;
            }
        }
    }


    public class ForestMountain : AdvancedBiome
    {
        public ForestMountain() : base("ForestMountain", new BiomeQualifications(1, 2, .5f, 1.75f, 1.25f, 2, 30))
        {
        }

        public override void PopulateChunk(OpenExperimentalWorldProvider openExperimentalWorldProvider, ChunkColumn c,
            float[] rth)
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

                var w = new Wool();
                w.Color = "green";
                c.SetBlock(x, y, z, w);
                c.SetHeight(x, z, (short) y);
                break;
            }
        }
    }


    public class WaterBiome : AdvancedBiome
    {
        public WaterBiome() : base("Water", new BiomeQualifications(0, 2, 1, 1.75f, 0, 0.5f
            , 30))
        {
        }

        public override async void PopulateChunk(OpenExperimentalWorldProvider openExperimentalWorldProvider,
            ChunkColumn c,
            float[] rth)
        {
            int stopheight =
                (int) Math.Floor(BiomeQualifications.baseheight + (rth[2] * BiomeQualifications.heightvariation));

            int sh = stopheight - BiomeQualifications.baseheight / 2;
            for (var x = 0; x < 16; x++)
            for (var z = 0; z < 16; z++)
            for (var y = 0; y < 255; y++)
            {
                if (y == 0)
                {
                    c.SetBlock(x, y, z, new Bedrock());
                    continue;
                }

                if (y <= sh - 3)
                {
                    c.SetBlock(x, y, z, new Stone());
                    continue;
                }

                if (y <= sh)
                {
                    var i = 0;
                    i = (new Random()).Next(0, 10);
                    if (i > 8) c.SetBlock(x, y, z, new Sand());
                    else if (i > 6) c.SetBlock(x, y, z, new Clay());
                    else if (i > 4) c.SetBlock(x, y, z, new Kelp());
                    else c.SetBlock(x, y, z, new Sand());
                    continue;
                }

                if (y <= stopheight)
                {
                    c.SetBlock(x, y, z, new FlowingWater());
                    continue;
                }

                c.SetBlock(x, y, z, new FlowingWater());
                c.SetHeight(x, z, (short) y);
                break;
            }

            // foreach (OpenPlayer p in openExperimentalWorldProvider.Level.GetSpawnedPlayers())
            // {
            //     if ((int) p.KnownPosition.X << 16 == c.X && (int) p.KnownPosition.Z << 16 == c.Z)
            //     {
            //         p.ForcedSendChunks();
            //         p.ForcedSendChunks();
            //     }
            // }
        }
    }


    public class Plains : AdvancedBiome
    {
        public Plains() : base("Plains", new BiomeQualifications(0.5f, 1.5f, 0.5f, 1.75f, 0.5f, 1f, 30))
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
                int sh= (int) Math.Floor(BiomeQualifications.baseheight + ((rth[2] )* BiomeQualifications.heightvariation))+(int)(HeightNoise.GetNoise(c.X * 16 + x, c.Z * 16 + z) * 4);
                // int sh= (int) Math.Floor(BiomeQualifications.baseheight + ((rth[2])* BiomeQualifications.heightvariation));

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
                    if (y < sh )
                    {
                        int r = (new Random()).Next(0, 3);
                        if(r == 0)c.SetBlock(x, y, z, new Stone());
                        if(r == 1)c.SetBlock(x, y, z, new Dirt());
                        if(r == 2)c.SetBlock(x, y, z, new Dirt());
                        if(r == 3)c.SetBlock(x, y, z, new Stone());
                        continue;
                    }

                    c.SetBlock(x, y, z, new Grass());
                    c.SetHeight(x, z, (short) y);
                    break;
                }
            }
        }
    }


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


    public class TropicalSeasonalForest : AdvancedBiome
    {
        public TropicalSeasonalForest() : base("TropicalSeasonalForest", new BiomeQualifications(0.5f, 1.25f, 0f,
            0.5f,
            0.5f, 1.5f
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

                c.SetBlock(x, y, z, new RedstoneOre());
                c.SetHeight(x, z, (short) y);
                break;
            }
        }
    }
}