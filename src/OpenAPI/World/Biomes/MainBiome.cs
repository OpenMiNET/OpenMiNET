using System;
using System.Threading.Tasks;
using MiNET.Blocks;
using MiNET.Worlds;

namespace OpenAPI.World
{
    /// <summary>
    /// Main biome
    /// </summary>
    public class MainBiome : AdvancedBiome
    {
        private float stoneBaseHeight = 3;
        // private float stoneBaseHeight = 50;
        private float stoneBaseNoise = 0.05f;
        private float stoneBaseNoiseHeight = 20;

        private float stoneMountainHeight = 2;
        // private float stoneMountainHeight = 48;
        private float stoneMountainFrequency = 0.008f;
        private float stoneMinHeight = 5;
        // private float stoneMinHeight = 20;

        private float dirtBaseHeight = 3;
        private float dirtNoise = 0.004f;
        private float dirtNoiseHeight = 10;
        private int waterLevel = 25;
        public MainBiome() : base("MAIN", new BiomeQualifications(0,2,0,2,0,2,40))
        {
            startheight = 90;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="openExperimentalWorldProvider"></param>
        /// <param name="chunk"></param>
        /// <param name="rtf"></param>
        public override void PopulateChunk(OpenExperimentalWorldProvider openExperimentalWorldProvider,
            ChunkColumn chunk, float[] rtf)
        {
            int trees = new Random().Next(0, 10);
            int[,] treeBasePositions = new int[trees, 2];

            for (int t = 0; t < trees; t++)
            {
                int x = new Random().Next(1, 16);
                int z = new Random().Next(1, 16);
                treeBasePositions[t, 0] = x;
                treeBasePositions[t, 1] = z;
            }

            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    int stoneHeight = (int) Math.Floor(stoneBaseHeight);
                    waterLevel = stoneHeight;
                    stoneHeight += GetNoise(chunk.X * 16 + x, chunk.Z * 16 + z, stoneMountainFrequency,
                        (int) Math.Floor(stoneMountainHeight));

                    if (stoneHeight < stoneMinHeight)
                        stoneHeight = (int) Math.Floor(stoneMinHeight);

                    stoneHeight += GetNoise(chunk.X * 16 + x, chunk.Z * 16 + z, stoneBaseNoise,
                        (int) Math.Floor(stoneBaseNoiseHeight));

                    int dirtHeight = stoneHeight + (int) Math.Floor(dirtBaseHeight);
                    dirtHeight += GetNoise(chunk.X * 16 + x, chunk.Z * 16 + z, dirtNoise,
                        (int) Math.Floor(dirtNoiseHeight));
                    // int riverint = GetNoise(chunk.X * 16 + x, chunk.Z * 16 + z, dirtNoise,
                    //     10);
                    // int riverheight = dirtHeight + GetNoise(chunk.X * 16 + x, chunk.Z * 16 + z, dirtNoise,
                    //     8);

                    // Console.WriteLine($" MORE: S{stoneHeight} D{dirtHeight} <<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
                    for (int y = 0; y < 255; y++)
                    {
                        //float y2 = Get3DNoise(chunk.X*16, y, chunk.Z*16, stoneBaseNoise, (int) Math.Floor(stoneBaseNoiseHeight));
                        if (y <= stoneHeight)
                        {
                            chunk.SetBlock(x, y, z, new Stone());

                            //Diamond ore
                            if (GetRandomNumber(0, 2500) < 5)
                            {
                                chunk.SetBlock(x, y, z, new DiamondOre());
                            }

                            //Coal Ore
                            if (GetRandomNumber(0, 1500) < 50)
                            {
                                chunk.SetBlock(x, y, z, new CoalOre());
                            }

                            //Iron Ore
                            if (GetRandomNumber(0, 2500) < 30)
                            {
                                chunk.SetBlock(x, y, z, new IronOre());
                            }

                            //Gold Ore
                            if (GetRandomNumber(0, 2500) < 20)
                            {
                                chunk.SetBlock(x, y, z, new GoldOre());
                            }
                        }

                        if (y < waterLevel) //FlowingWater :)
                        {
                            if (chunk.GetBlockId(x, y, z) == 2 || chunk.GetBlockId(x, y, z) == 3) //Grass or Dirt?
                            {
                                if (GetRandomNumber(1, 40) == 5 && y < waterLevel - 4)
                                    chunk.SetBlock(x, y, z, new Clay()); //Clay
                                else
                                    chunk.SetBlock(x, y, z, new Sand()); //Sand
                            }

                            if (y < waterLevel - 3)
                                chunk.SetBlock(x, y + 1, z, new FlowingWater()); //FlowingWater
                        }

                        // if (riverint <= 1 && y > dirtHeight && riverheight > y)
                        // {
                        //     Console.WriteLine($"WATER AT {x} {y} {z}");
                        //     for (int py = 0; py < riverint; py++)
                        //     {
                        //         chunk.SetBlock(x, y - py,z,new Water());
                        //     }
                        //     
                        // }

                        if (y <= dirtHeight && y >= stoneHeight)
                        {
                            if (y == dirtHeight)
                            {
                                chunk.SetBlock(x, y, z, new Grass()); //Grass Block
                                chunk.SetHeight(x, z, (short) (y));
                                if (y > waterLevel)
                                {
                                    //Grass
                                    if (GetRandomNumber(0, 5) == 2)
                                    {
                                        chunk.SetBlock(x, y + 1, z, new Tallgrass() {TallGrassType = "tall"});
                                    }

                                    //flower
                                    if (GetRandomNumber(0, 65) == 8)
                                    {
                                        int meta = GetRandomNumber(0, 8);
                                        //chunk.SetBlock(x, y + 2, z, 38, (byte) meta);
                                        chunk.SetBlock(x, y + 1, z, new RedFlower());
                                    }

                                    for (int pos = 0; pos < trees; pos++)
                                    {
                                        if (treeBasePositions[pos, 0] < 14 && treeBasePositions[pos, 0] > 4 &&
                                            treeBasePositions[pos, 1] < 14 &&
                                            treeBasePositions[pos, 1] > 4)
                                        {
                                            if (y < waterLevel + 2)
                                                break;
                                            if (chunk.GetBlockId(treeBasePositions[pos, 0], y ,
                                                treeBasePositions[pos, 1]) == 2)
                                            {
                                                if (y == dirtHeight)
                                                    GenerateTree(chunk, treeBasePositions[pos, 0], y,
                                                        treeBasePositions[pos, 1]);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                chunk.SetBlock(x, y, z, new Dirt()); //Dirt
                                
                            }
                        }

                        if (y == 0)
                        {
                            chunk.SetBlock(x, y, z, new Bedrock());
                        }
                    }
                }
            }
            //
            // return chunk;
        }
        
        private void GenerateTree(ChunkColumn chunk, int x, int treebase, int z)
        {
            int treeheight = GetRandomNumber(4, 5);

            chunk.SetBlock(x, treebase + treeheight + 2, z, new Leaves()); //Top leave

            chunk.SetBlock(x, treebase + treeheight + 1, z + 1, new Leaves());
            chunk.SetBlock(x, treebase + treeheight + 1, z - 1, new Leaves());
            chunk.SetBlock(x + 1, treebase + treeheight + 1, z, new Leaves());
            chunk.SetBlock(x - 1, treebase + treeheight + 1, z, new Leaves());

            chunk.SetBlock(x, treebase + treeheight, z + 1, new Leaves());
            chunk.SetBlock(x, treebase + treeheight, z - 1, new Leaves());
            chunk.SetBlock(x + 1, treebase + treeheight, z, new Leaves());
            chunk.SetBlock(x - 1, treebase + treeheight, z, new Leaves());

            chunk.SetBlock(x + 1, treebase + treeheight, z + 1, new Leaves());
            chunk.SetBlock(x - 1, treebase + treeheight, z - 1, new Leaves());
            chunk.SetBlock(x + 1, treebase + treeheight, z - 1, new Leaves());
            chunk.SetBlock(x - 1, treebase + treeheight, z + 1, new Leaves());

            for (int i = 0; i <= treeheight; i++)
            {
                chunk.SetBlock(x, treebase + i, z, new Log());
            }
        } 
        
        private static readonly Random getrandom = new Random();
        private static readonly object syncLock = new object();

        private static int GetRandomNumber(int min, int max)
        {
            lock (syncLock)
            {
                // synchronize
                return getrandom.Next(min, max);
            }
        }
    }
}