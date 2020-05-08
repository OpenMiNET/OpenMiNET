using MiNET.Blocks;
using MiNET.Worlds;

namespace OpenAPI.World
{
    public class ForestBiome : AdvancedBiome
    {
        public ForestBiome() : base("ForestBiome", new BiomeQualifications(.5f, 1f, 0, .5f, 30))
        {
        }

        public override void PopulateChunk(ChunkColumn c, float rain, float temp)
        {
            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    c.SetBlock(x,0,z,new Bedrock());
                    c.SetBlock(x,1,z,new GreenGlazedTerracotta());
                    c.SetHeight(x,z,1);
                }
            }
        }
    }

    public class WarmForestBiome : AdvancedBiome
    {
        public WarmForestBiome() : base("WarmForestBiome", new BiomeQualifications(.5f, 1f, .6f, 1f, 30))
        {
        }

        public override void PopulateChunk(ChunkColumn c, float rain, float temp)
        {
            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    c.SetBlock(x,0,z,new Bedrock());
                    c.SetBlock(x, 1, z, new RedstoneBlock());
                    c.SetHeight(x,z,1);
                }
            }
        }
    }

    public class HotForestBiome : AdvancedBiome
    {
        public HotForestBiome() : base("HotForestBiome", new BiomeQualifications(.5f, 1f, 1.1f, 2f, 30))
        {
        }

        public override void PopulateChunk(ChunkColumn c, float rain, float temp)
        {
            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    c.SetBlock(x,0,z,new Bedrock());
                    c.SetBlock(x,1,z,new RedSandstone());
                    c.SetHeight(x,z,1);
                }
            }
        }
    }
    public class WaterBiome : AdvancedBiome
    {
        public WaterBiome() : base("WaterBiome", new BiomeQualifications(.8f, 2f, 1f, 2f, 30))
        {
        }

        public override void PopulateChunk(ChunkColumn c, float rain, float temp)
        {
            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    c.SetBlock(x,0,z,new Bedrock());
                    c.SetBlock(x,1,z,new Water());
                    c.SetHeight(x,z,1);
                }
            }
        }
    }
    public class IcyBiome : AdvancedBiome
    {
        public IcyBiome() : base("IcyBiome", new BiomeQualifications(.8f, 2f, 0f, 1f, 30))
        {
        }

        public override void PopulateChunk(ChunkColumn c, float rain, float temp)
        {
            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    c.SetBlock(x,0,z,new Bedrock());
                    c.SetBlock(x,1,z,new Ice());
                    c.SetHeight(x,z,1);
                }
            }
        }
    }
}