using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Worlds;

namespace OpenAPI.World
{
    public class NukkitGenerator : IWorldGenerator
    {
        private int generatedchunks;


        private readonly FastNoise HeightNoise; // Create a FastNoise object
        private readonly FastNoise TempatureNoise; // Create a FastNoise object
        private readonly FastNoise RainNoise; // Create a FastNoise object
        private readonly FastNoise WaterNoise; // Create a FastNoise object
        private readonly FastNoise MainNoise; // Create a FastNoise object
        private readonly FastNoise MainNoise2; // Create a FastNoise object
        private readonly FastNoise MainNoise3; // Create a FastNoise object
        private readonly FastNoise MainNoise4; // Create a FastNoise object
        private readonly Random Random1 = new Random(158985); // Create a FastNoise object
        private readonly Random Random2 = new Random(157659); // Create a FastNoise object

        public NukkitGenerator(Dimension dimension)
        {
            Dimension = dimension;
            HeightNoise = new FastNoise(123123);
            // myNoise.SetNoiseType(FastNoise.NoiseType.Perlin); // Set the desired noise type
            HeightNoise.SetNoiseType(FastNoise.NoiseType.Cubic); // Set the desired noise type
            // HeightNoise.SetFrequency(0.015f);
            HeightNoise.SetFrequency(0.010f);
            HeightNoise.SetGradientPerturbAmp(15.0f);
            TempatureNoise = new FastNoise(1337);
            TempatureNoise.SetNoiseType(FastNoise.NoiseType.Cubic); // Set the desired noise type
            TempatureNoise.SetFrequency(0.005f);
            MainNoise = new FastNoise(1311232187);
            MainNoise.SetNoiseType(FastNoise.NoiseType.ValueFractal); // Set the desired noise type
            MainNoise.SetFrequency(0.35f);
            MainNoise.SetFractalLacunarity(.5f);
            MainNoise.SetFractalGain(.0009f);
            MainNoise2 = new FastNoise(131128777);
            MainNoise2.SetNoiseType(FastNoise.NoiseType.Cubic); // Set the desired noise type
            MainNoise2.SetFrequency(0.35f);
            MainNoise3 = new FastNoise(1312976537);
            MainNoise3.SetNoiseType(FastNoise.NoiseType.Cubic); // Set the desired noise type
            MainNoise3.SetFrequency(0.35f);
            MainNoise4 = new FastNoise(1312976537);
            MainNoise4.SetNoiseType(FastNoise.NoiseType.Cubic); // Set the desired noise type
            MainNoise4.SetFrequency(0.35f);
            RainNoise = new FastNoise(133337);
            RainNoise.SetNoiseType(FastNoise.NoiseType.Cubic); // Set the desired noise type
            RainNoise.SetFrequency(0.005f);
            WaterNoise = new FastNoise(1231551);
            WaterNoise.SetNoiseType(FastNoise.NoiseType.Cubic); // Set the desired noise type
            WaterNoise.SetFrequency(0.005f);
            // myNoise.SetInterp(FastNoise.Interp.Quintic);

            for (int i = -2; i <= 2; ++i)
            {
                for (int j = -2; j <= 2; ++j)
                {
                    biomeWeights[i + 2 + (j + 2) * 5] = (float) (10.0F / Math.Sqrt((float) (i * i + j * j) + 0.2F));
                }
            }
        }

        public int Seed { get; set; } = 1233;

        public List<Block> BlockLayers { get; set; }

        public Dimension Dimension { get; set; }

        public void Initialize()
        {
            // BlockLayers = SuperflatGenerator.ParseSeed(Seed);
        }

        private static float[] biomeWeights = new float[25];


        public ChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates)
        {
            StackTrace stackTrace = new StackTrace();
            Console.WriteLine("CALLED FROM 111>>>>" + stackTrace.GetFrame(1).GetMethod().Name);
            Console.WriteLine("CALLED FROM 2222>>>>" + stackTrace.GetFrame(2).GetMethod().Name);
            Console.WriteLine("CALLED FROM 333>>>>" + stackTrace.GetFrame(3).GetMethod().Name);
            Console.WriteLine("CALLED FROM 333>>>>" + stackTrace.GetFrame(4).GetMethod().Name);
            Console.WriteLine("CALLED FROM 333>>>>" + stackTrace.GetFrame(5).GetMethod().Name);
            Console.WriteLine("CALLED FROM 333>>>>" + stackTrace.GetFrame(6).GetMethod().Name);
            Console.WriteLine("CALLED FROM 333>>>>" + stackTrace.GetFrame(7).GetMethod().Name);
            Console.WriteLine("CALLED FROM 333>>>>" + stackTrace.GetFrame(8).GetMethod().Name);
            Console.WriteLine("CALLED FROM 333>>>>" + stackTrace.GetFrame(9).GetMethod().Name);
            Console.WriteLine("CALLED FROM 333>>>>" + stackTrace.GetFrame(10).GetMethod().Name);
            var t = new Stopwatch();
            t.Start();
            var chunk = new ChunkColumn();
            int chunkX = chunkCoordinates.X;
            int chunkZ = chunkCoordinates.Z;
            chunk.X = chunkCoordinates.X;
            chunk.Z = chunkCoordinates.Z;

            int baseX = chunk.X << 4;
            int baseZ = chunk.Z << 4;
            Random nukkitrandom = new Random(chunk.X * chunk.Z);


            var realBiome = new BiomeSelector(Seed);


            //generate base noise values
            // float depthRegion = this.MainNoise.GetNoise(chunkX * 4, chunkZ * 4);
            // this.depthRegion.set(depthRegion);
            // float[] mainNoiseRegion = this.mainPerlinNoise.generateNoiseOctaves(this.mainNoiseRegion.get(), chunkX * 4, 0, chunkZ * 4, 5, 33, 5, 684.412f / 60f, 684.412f / 160f, 684.412f / 60f);
            // this.mainNoiseRegion.set(mainNoiseRegion);
            // float[] minLimitRegion = this.minLimitPerlinNoise.generateNoiseOctaves(this.minLimitRegion.get(), chunkX * 4, 0, chunkZ * 4, 5, 33, 5, 684.412f, 684.412f, 684.412f);
            // this.minLimitRegion.set(minLimitRegion);
            // float[] maxLimitRegion = this.maxLimitPerlinNoise.generateNoiseOctaves(this.maxLimitRegion.get(), chunkX * 4, 0, chunkZ * 4, 5, 33, 5, 684.412f, 684.412f, 684.412f);
            // this.maxLimitRegion.set(maxLimitRegion);
            // float[] heightMap = this.heightMap.get();

            //generate heightmap and smooth biome heights
            int horizCounter = 0;
            int vertCounter = 0;
            float[] heightMap = new float[256*256];
            for (int xSeg = 0; xSeg < 5; ++xSeg)
            {
                for (int zSeg = 0; zSeg < 5; ++zSeg)
                {
                    float heightVariationSum = 0.0F;
                    float baseHeightSum = 0.0F;
                    float biomeWeightSum = 0.0F;
                    Biome biome = realBiome.pickBiome(baseX + (xSeg * 4), baseZ + (zSeg * 4));
                    AdvancedBiome bb = AdvancedBiome.GetBiome(biome.Id);
                    for (int xSmooth = -2; xSmooth <= 2; ++xSmooth)
                    {
                        for (int zSmooth = -2; zSmooth <= 2; ++zSmooth)
                        {
                            Biome biome1 = realBiome.pickBiome(baseX + (xSeg * 4) + xSmooth,
                                baseZ + (zSeg * 4) + zSmooth);

                            AdvancedBiome bb1 = AdvancedBiome.GetBiome(biome1.Id);
                            float baseHeight = bb1.startheight;
                            float heightVariation = bb1.BiomeQualifications.heightvariation;

                            float scaledWeight = biomeWeights[xSmooth + 2 + (zSmooth + 2) * 5] / (baseHeight + 2.0F);

                            if (baseHeight > bb.startheight)
                            {
                                scaledWeight /= 2.0F;
                            }

                            heightVariationSum += heightVariation * scaledWeight;
                            baseHeightSum += baseHeight * scaledWeight;
                            biomeWeightSum += scaledWeight;
                        }
                    }

                    heightVariationSum = heightVariationSum / biomeWeightSum;
                    baseHeightSum = baseHeightSum / biomeWeightSum;
                    heightVariationSum = heightVariationSum * 0.9F + 0.1F;
                    baseHeightSum = (baseHeightSum * 4.0F - 1.0F) / 8.0F;
                    float depthNoise = MainNoise.GetNoise(xSeg, zSeg); /*/ 8000.0f;*/

                    if (depthNoise < 0.0f)
                    {
                        depthNoise = -depthNoise * 0.3f;
                    }

                    depthNoise = depthNoise * 3.0f - 2.0f;

                    if (depthNoise < 0.0f)
                    {
                        depthNoise = depthNoise / 2.0f;

                        if (depthNoise < -1.0f)
                        {
                            depthNoise = -1.0f;
                        }

                        depthNoise = depthNoise / 1.4f;
                        depthNoise = depthNoise / 2.0f;
                    }
                    else
                    {
                        if (depthNoise > 1.0f)
                        {
                            depthNoise = 1.0f;
                        }

                        depthNoise = depthNoise / 8.0f;
                    }

                    ++vertCounter;
                    float baseHeightClone = baseHeightSum;
                    float heightVariationClone = heightVariationSum;
                    baseHeightClone = baseHeightClone + depthNoise * 0.2f;
                    baseHeightClone = baseHeightClone * 8.5f / 8.0f;
                    float baseHeightFactor = 8.5f + baseHeightClone * 4.0f;

                    for (int ySeg = 0; ySeg < 33; ++ySeg)
                    {
                        float baseScale = ((float) ySeg - baseHeightFactor) * 12f * 128.0f / 256.0f /
                                          heightVariationClone;

                        if (baseScale < 0.0f)
                        {
                            baseScale *= 4.0f;
                        }

                        float minScaled = MainNoise2.GetNoise(xSeg, zSeg);
                        float maxScaled = MainNoise3.GetNoise(xSeg, zSeg);
                        float noiseScaled = (MainNoise4.GetNoise(xSeg, zSeg) / 10.0f + 1.0f) / 2.0f;
                        float clamp = Math.Clamp(minScaled, maxScaled, noiseScaled) - baseScale;

                        if (ySeg > 29)
                        {
                            float yScaled = ((float) (ySeg - 29) / 3.0F);
                            clamp = clamp * (1.0f - yScaled) + -10.0f * yScaled;
                        }

                        heightMap[horizCounter] = clamp;
                        ++horizCounter;
                    }
                }
            }

            //place blocks
            for (int xSeg = 0; xSeg < 4; ++xSeg)
            {
                int xScale = xSeg * 5;
                int xScaleEnd = (xSeg + 1) * 5;

                for (int zSeg = 0; zSeg < 4; ++zSeg)
                {
                    int zScale1 = (xScale + zSeg) * 33;
                    int zScaleEnd1 = (xScale + zSeg + 1) * 33;
                    int zScale2 = (xScaleEnd + zSeg) * 33;
                    int zScaleEnd2 = (xScaleEnd + zSeg + 1) * 33;

                    for (int ySeg = 0; ySeg < 32; ++ySeg)
                    {
                        double height1 = heightMap[zScale1 + ySeg];
                        double height2 = heightMap[zScaleEnd1 + ySeg];
                        double height3 = heightMap[zScale2 + ySeg];
                        double height4 = heightMap[zScaleEnd2 + ySeg];
                        double height5 = (heightMap[zScale1 + ySeg + 1] - height1) * 0.125f;
                        double height6 = (heightMap[zScaleEnd1 + ySeg + 1] - height2) * 0.125f;
                        double height7 = (heightMap[zScale2 + ySeg + 1] - height3) * 0.125f;
                        double height8 = (heightMap[zScaleEnd2 + ySeg + 1] - height4) * 0.125f;

                        for (int yIn = 0; yIn < 8; ++yIn)
                        {
                            double baseIncr = height1;
                            double baseIncr2 = height2;
                            double scaleY = (height3 - height1) * 0.25f;
                            double scaleY2 = (height4 - height2) * 0.25f;

                            for (int zIn = 0; zIn < 4; ++zIn)
                            {
                                double scaleZ = (baseIncr2 - baseIncr) * 0.25f;
                                double scaleZ2 = baseIncr - scaleZ;

                                for (int xIn = 0; xIn < 4; ++xIn)
                                {
                                    if ((scaleZ2 += scaleZ) > 0.0f)
                                    {
                                        chunk.SetBlock(xSeg * 4 + zIn, ySeg * 8 + yIn, zSeg * 4 + xIn, new Stone());
                                    }
                                    else if (ySeg * 8 + yIn <= seaHeight)
                                    {
                                        chunk.SetBlock(xSeg * 4 + zIn, ySeg * 8 + yIn, zSeg * 4 + xIn, new Water());
                                    }
                                }

                                baseIncr += scaleY;
                                baseIncr2 += scaleY2;
                            }

                            height1 += height5;
                            height2 += height6;
                            height3 += height7;
                            height4 += height8;
                        }
                    }
                }
            }

            // Biome biome = realBiome.pickBiome(baseX + (xSeg * 4), baseZ + (zSeg * 4));
            // AdvancedBiome bb = AdvancedBiome.GetBiome(biome.Id);

            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    Biome biome = realBiome.pickBiome(baseX | x, baseZ | z);

                    chunk.SetBiome(x, z, (byte) biome.Id);
                }
            }

            //populate chunk
            // for (Populator populator : this.generationPopulators) {
            //     populator.populate(this.level, chunkX, chunkZ, this.nukkitRandom, chunk);
            // }


            // PopulateChunk(chunk);
            t.Stop();
            Console.WriteLine($"THIS TOOK {t.Elapsed}");
            // var random = new Random((chunk.X * 397) ^ chunk.Z);
            // if (random.NextDouble() > 0.99)
            //     GenerateLake(random, chunk,
            //         Dimension == Dimension.Overworld
            //             ? new Water()
            //             : Dimension == Dimension.Nether
            //                 ? new Lava()
            //                 : (Block) new Air());
            // else if (random.NextDouble() > 0.97)
            //     GenerateGlowStone(random, chunk);
            return chunk;
        }

        public static int seaHeight = 64;

        private int FindGroundLevel()
        {
            var num = 0;
            var flag = false;
            foreach (var blockLayer in BlockLayers)
            {
                if (flag && blockLayer is Air)
                    return num - 1;
                if (blockLayer.IsSolid)
                    flag = true;
                ++num;
            }

            return !flag ? -1 : num - 1;
        }

        // public void RePopulateChunk(ChunkColumn chunk, float temp, float rain)
        // {
        //     var localSeed1 = Random1.Next();
        //     var localSeed2 = Random1.Next();
        //     var realBiome = new BiomeSelector(Seed);
        //
        //     int realX = chunk.X << 4;
        //     int realZ = chunk.Z << 4;
        //     for (int x = 0; x < 16; ++x)
        //     {
        //         for (int z = 0; z < 16; ++z)
        //         {
        //             Biome biome = realBiome.pickBiome(realX + x, realZ + z);
        //             OpenBiome openBiome;
        //             openBiome.preCover(realX | x, realZ | z);
        //             int coverBlock = biome.getCoverBlock() << 4;
        //
        //             boolean hasCovered = false;
        //             int realY;
        //             //start one below build limit in case of cover blocks
        //             for (int y = 254; y > 32; y--)
        //             {
        //                 if (chunk.getFullBlock(x, y, z) == STONE)
        //                 {
        //                     COVER:
        //                     if (!hasCovered)
        //                     {
        //                         if (y >= Normal.seaHeight)
        //                         {
        //                             chunk.setFullBlockId(x, y + 1, z, coverBlock);
        //                             int surfaceDepth = biome.getSurfaceDepth(y);
        //                             for (int i = 0; i < surfaceDepth; i++)
        //                             {
        //                                 realY = y - i;
        //                                 if (chunk.getFullBlock(x, realY, z) == STONE)
        //                                 {
        //                                     chunk.setFullBlockId(x, realY, z,
        //                                         (biome.getSurfaceBlock(realY) << 4) | biome.getSurfaceMeta(realY));
        //                                 }
        //                                 else break
        //
        //                                 COVER;
        //                             }
        //
        //                             y -= surfaceDepth;
        //                         }
        //
        //                         int groundDepth = biome.getGroundDepth(y);
        //                         for (int i = 0; i < groundDepth; i++)
        //                         {
        //                             realY = y - i;
        //                             if (chunk.getFullBlock(x, realY, z) == STONE)
        //                             {
        //                                 chunk.setFullBlockId(x, realY, z,
        //                                     (biome.getGroundBlock(realY) << 4) | biome.getGroundMeta(realY));
        //                             }
        //                             else break
        //
        //                             COVER;
        //                         }
        //
        //                         //don't take all of groundDepth away because we do y-- in the loop
        //                         y -= groundDepth - 1;
        //                     }
        //
        //                     hasCovered = true;
        //                 }
        //                 else
        //                 {
        //                     if (hasCovered)
        //                     {
        //                         //reset it if this isn't a valid stone block (allows us to place ground cover on top and below overhangs)
        //                         hasCovered = false;
        //                     }
        //                 }
        //             }
        //         }
        //     }
        //
        //
        //     this.minLimitPerlinNoise = new NoiseGeneratorOctavesF(random, 16);
        //     this.maxLimitPerlinNoise = new NoiseGeneratorOctavesF(random, 16);
        //     this.mainPerlinNoise = new NoiseGeneratorOctavesF(random, 8);
        //     this.surfaceNoise = new NoiseGeneratorPerlinF(random, 4);
        //     this.scaleNoise = new NoiseGeneratorOctavesF(random, 10);
        //     this.depthNoise = new NoiseGeneratorOctavesF(random, 16);
        //
        //     //this should run before all other populators so that we don't do things like generate ground cover on bedrock or something
        //     PopulatorGroundCover cover = new PopulatorGroundCover();
        //     this.generationPopulators.add(cover);
        //
        //     PopulatorBedrock bedrock = new PopulatorBedrock();
        //     this.generationPopulators.add(bedrock);
        //
        //     PopulatorOre ores = new PopulatorOre();
        //     ores.setOreTypes(new OreType[]
        //     {
        //         new OreType(Block.get(BlockID.COAL_ORE), 20, 17, 0, 128),
        //         new OreType(Block.get(BlockID.IRON_ORE), 20, 9, 0, 64),
        //         new OreType(Block.get(BlockID.REDSTONE_ORE), 8, 8, 0, 16),
        //         new OreType(Block.get(BlockID.LAPIS_ORE), 1, 7, 0, 16),
        //         new OreType(Block.get(BlockID.GOLD_ORE), 2, 9, 0, 32),
        //         new OreType(Block.get(BlockID.DIAMOND_ORE), 1, 8, 0, 16),
        //         new OreType(Block.get(BlockID.DIRT), 10, 33, 0, 128),
        //         new OreType(Block.get(BlockID.GRAVEL), 8, 33, 0, 128),
        //         new OreType(Block.get(BlockID.STONE, BlockStone.GRANITE), 10, 33, 0, 80),
        //         new OreType(Block.get(BlockID.STONE, BlockStone.DIORITE), 10, 33, 0, 80),
        //         new OreType(Block.get(BlockID.STONE, BlockStone.ANDESITE), 10, 33, 0, 80)
        //     });
        //     this.populators.add(ores);
        //
        //     PopulatorCaves caves = new PopulatorCaves();
        //     this.populators.add(caves);
        //
        //     //TODO: fix ravines
        //     //PopulatorRavines ravines = new PopulatorRavines();
        //     //this.populators.add(ravines);
        //
        //
        //     // var b = getBiomeFromTemp(temp, rain);
        //     var b = getBiomeFromTemp(temp, rain);
        //     var sx = chunk.X;
        //     var sz = chunk.Z;
        //     var random = new Random((sx << 8) ^ sz);
        //     for (var bx = 0; bx < 16; ++bx)
        //     {
        //         for (var bz = 0; bz < 16; ++bz)
        //         {
        //             generatedchunks++;
        //             var rnd = new Random(bx + bz);
        //             var tx = sx * 16 + bx;
        //             var tz = sz * 16 + bz;
        //             var height = Math.Abs(HeightNoise.GetNoise(tx, tz));
        //             var maxheight = (int) (height * b.heightvariation) + b.startheight;
        //             var grassheightstart = maxheight - rnd.Next(2, 6) - 1;
        //             var grassheightstop = grassheightstart + (maxheight - grassheightstart) - 1;
        //             if (bx == 0 && bz == 0)
        //                 Console.WriteLine(
        //                     $"YOaaaaaaaaaaaaaaaaaaaaaaaaaaaaaOO #{generatedchunks} && {generatedchunks / 16}&& {generatedchunks / 256}>>> \n" +
        //                     $">>{sx * 16}|{sz * 16}\n" +
        //                     $">>{tx}|{tz}|H{height}|M{maxheight}|GS{grassheightstart}|GSTP{grassheightstop}");
        //             for (var by = 0; by < maxheight; ++by)
        //             {
        //                 Block block = new Stone();
        //                 if (by == 0) block = new Bedrock();
        //                 else if (by == 1 || by < grassheightstart)
        //                     block = new Stone();
        //                 else if (by >= grassheightstart && by < grassheightstop)
        //                     block = new Dirt();
        //                 else if (by == grassheightstop) block = new Grass();
        //                 else
        //                 {
        //                     continue;
        //                 }
        //
        //                 chunk.SetBlock(bx, by, bz, block);
        //                 chunk.SetSkyLight(bx, by, bz, 0);
        //             }
        //
        //             chunk.SetHeight(bx, bz, (short) maxheight);
        //             // for (var by2 = maxheight + Dimension == Dimension.Overworld ? 1 : 0; by2 >= 0; --by2)
        //             //     chunk.SetSkyLight(bx, by2, bz, 0);
        //             chunk.SetBiome(bx, bz, 1);
        //         }
        //     }
        // }


        public void PopulateChunk(ChunkColumn chunk)
        {
            var sx = chunk.X;
            var sz = chunk.Z;
            var blockLayers = BlockLayers;
            float tempn = Math.Abs(HeightNoise.GetNoise(sx * 16, sz * 16));
            float rainn = Math.Abs(HeightNoise.GetNoise(sx * 16, sz * 16));
            float temp = 2 * tempn;

            // RePopulateChunk(chunk, temp, rainn);
            // OpenServer.FastThreadPool.QueueUserWorkItem(() => { RePopulateChunk(chunk); });
            // for (var bx = 0; bx < 16; ++bx)
            // {
            //     for (var bz = 0; bz < 16; ++bz)
            //     {
            //         chunk.SetBlock(bx, 0, bz, new Bedrock());
            //
            //         chunk.SetHeight(bx, bz, 0);
            //         for (var by2 = 0 + Dimension == Dimension.Overworld ? 1 : 0; by2 >= 0; --by2)
            //             chunk.SetSkyLight(bx, by2, bz, 0);
            //         chunk.SetBiome(bx, bz, 1);
            //     }
            // }
        }

        public static List<Block> ParseSeed(string inputSeed)
        {
            if (string.IsNullOrEmpty(inputSeed))
                return new List<Block>();
            var blockList = new List<Block>();
            foreach (var str in inputSeed.Split(';')[1].Split(','))
            {
                var strArray1 = str.Replace("minecraft:", "").Split('*');
                var strArray2 = strArray1[0].Split(':');
                var num = 1;
                if (strArray1.Length > 1)
                {
                    num = int.Parse(strArray1[0]);
                    strArray2 = strArray1[1].Split(':');
                }

                if (strArray2.Length != 0)
                {
                    byte result1;
                    var block = !byte.TryParse(strArray2[0], out result1)
                        ? BlockFactory.GetBlockByName(strArray2[0])
                        : BlockFactory.GetBlockById(result1);
                    byte result2;
                    if (strArray2.Length > 1 && byte.TryParse(strArray2[1], out result2))
                        block.Metadata = result2;
                    if (block != null)
                        for (var index = 0; index < num; ++index)
                            blockList.Add(block);
                    else
                        throw new Exception("Expected block, but didn't fine one for pattern " + str + ", " +
                                            string.Join("^", strArray2) + " ");
                }
            }

            return blockList;
        }
    }
}