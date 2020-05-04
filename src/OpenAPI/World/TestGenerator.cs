using System;
using System.Collections.Generic;
using System.Numerics;
using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Worlds;

namespace OpenAPI.World
{
    public class TestGenerator : IWorldGenerator
    {
        private int generatedchunks;


        private readonly FastNoise HeightNoise; // Create a FastNoise object
        private readonly FastNoise TempatureNoise; // Create a FastNoise object
        private readonly FastNoise RainNoise; // Create a FastNoise object
        private readonly FastNoise WaterNoise; // Create a FastNoise object

        public TestGenerator(Dimension dimension)
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
            RainNoise = new FastNoise(133337);
            RainNoise.SetNoiseType(FastNoise.NoiseType.Cubic); // Set the desired noise type
            RainNoise.SetFrequency(0.005f);
            WaterNoise = new FastNoise(1231551);
            WaterNoise.SetNoiseType(FastNoise.NoiseType.Cubic); // Set the desired noise type
            WaterNoise.SetFrequency(0.005f);
            // myNoise.SetInterp(FastNoise.Interp.Quintic);
        }

        public string Seed { get; set; }

        public List<Block> BlockLayers { get; set; }

        public Dimension Dimension { get; set; }

        public void Initialize()
        {
            BlockLayers = SuperflatGenerator.ParseSeed(Seed);
        }

        public ChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates)
        {
            var chunk = new ChunkColumn();
            chunk.X = chunkCoordinates.X;
            chunk.Z = chunkCoordinates.Z;
            PopulateChunk(chunk);
            var random = new Random((chunk.X * 397) ^ chunk.Z);
            if (random.NextDouble() > 0.99)
                GenerateLake(random, chunk,
                    Dimension == Dimension.Overworld
                        ? new Water()
                        : Dimension == Dimension.Nether
                            ? new Lava()
                            : (Block) new Air());
            else if (random.NextDouble() > 0.97)
                GenerateGlowStone(random, chunk);
            return chunk;
        }

        private void GenerateGlowStone(Random random, ChunkColumn chunk)
        {
            if (Dimension != Dimension.Nether || FindGroundLevel() < 0)
                return;
            var vector2_1 = new Vector2(7f, 8f);
            for (var bx = 0; bx < 16; ++bx)
            for (var bz = 0; bz < 16; ++bz)
            {
                var vector2_2 = new Vector2(bx, bz);
                if (random.Next((int) Vector2.DistanceSquared(vector2_1, vector2_2)) < 1)
                {
                    chunk.SetBlock(bx, BlockLayers.Count - 2, bz, new Glowstone());
                    if (random.NextDouble() > 0.85)
                    {
                        chunk.SetBlock(bx, BlockLayers.Count - 3, bz, new Glowstone());
                        if (random.NextDouble() > 0.5)
                            chunk.SetBlock(bx, BlockLayers.Count - 4, bz, new Glowstone());
                    }
                }
            }
        }

        private void GenerateLake(Random random, ChunkColumn chunk, Block block)
        {
            var groundLevel = FindGroundLevel();
            if (groundLevel < 0)
                return;
            var vector2_1 = new Vector2(7f, 8f);
            for (var bx = 0; bx < 16; ++bx)
            for (var bz = 0; bz < 16; ++bz)
            {
                var vector2_2 = new Vector2(bx, bz);
                if (random.Next((int) Vector2.DistanceSquared(vector2_1, vector2_2)) < 4)
                {
                    if (Dimension == Dimension.Overworld)
                    {
                        chunk.SetBlock(bx, groundLevel, bz, block);
                    }
                    else if (Dimension == Dimension.Nether)
                    {
                        chunk.SetBlock(bx, groundLevel, bz, block);
                        if (random.Next(30) == 0)
                            for (var by = groundLevel; by < BlockLayers.Count - 1; ++by)
                                chunk.SetBlock(bx, by, bz, block);
                    }
                    else if (Dimension == Dimension.TheEnd)
                    {
                        for (var by = 0; by < BlockLayers.Count; ++by)
                            chunk.SetBlock(bx, by, bz, new Air());
                    }
                }
                else if (Dimension == Dimension.TheEnd &&
                         random.Next((int) Vector2.DistanceSquared(vector2_1, vector2_2)) < 15)
                {
                    chunk.SetBlock(bx, groundLevel, bz, new Air());
                }
            }
        }

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

        public class AdvancedBiome
        {
            public String name;
            public float startrain;//0 - 1
            public float stoprain;
            public float starttemp;//-1 - 1
            public float stoptemp;
            public int heightvariation;
            public int startheight = 80;
            public bool waterbiome = false;

            public AdvancedBiome(string name, float startrain, float stoprain, float starttemp, float stoptemp, int heightvariation)
            {
                this.name = name;
                this.startrain = startrain;
                this.stoprain = stoprain;
                this.starttemp = starttemp;
                this.stoptemp = stoptemp;
                this.heightvariation = heightvariation;
            }

            public bool check(float temp, float rain)
            {
                return (startrain <= rain && stoprain >= rain && starttemp <= temp && stoptemp >= temp);
            }
        }
        
        public List<AdvancedBiome> BiomeList = new List<AdvancedBiome>();
        
        public void registerBiomes()
        {
            BiomeList.Add(new AdvancedBiome("Snow",0,1,-1,0.05f,30));
            BiomeList.Add(new AdvancedBiome("Mountain",0,1,0,0.2f,60));
            BiomeList.Add(new AdvancedBiome("Plain",0,1,.5f,0.8f,5));
            BiomeList.Add(new AdvancedBiome("DEFAULT",0,1,0f,1f,15));
            
        }
        
        public AdvancedBiome getBiomeFromTemp(float temp, float downfall)
        {
            foreach (var b in BiomeList)
            {
                if (b.check(temp, downfall)) return b;
            }

            return new AdvancedBiome("DEFAULT", 0, 1, 0f, 1f, 15);
        }
        
        public void RePopulateChunk(ChunkColumn chunk, float temp, float rain)
        {
            var b = getBiomeFromTemp(temp, rain);
            var sx = chunk.X;
            var sz = chunk.Z;
            for (var bx = 0; bx < 16; ++bx)
            {
                for (var bz = 0; bz < 16; ++bz)
                {
                    generatedchunks++;
                    var rnd = new Random(bx + bz);
                    var tx = sx * 16 + bx;
                    var tz = sz * 16 + bz;
                    var height = Math.Abs(HeightNoise.GetNoise(tx, tz));
                    var maxheight = (int) (height * b.heightvariation) + b.startheight;
                    var grassheightstart = 128 + maxheight - rnd.Next(2, 6) - 1;
                    var grassheightstop = 128 + grassheightstart + (maxheight - grassheightstart) - 1;
                    if (bx == 0 && bz == 0)
                        Console.WriteLine($"YOaaaaaaaaaaaaaaaaaaaaaaaaaaaaaOO #{generatedchunks} && {generatedchunks/16}&& {generatedchunks/256}>>> \n" +
                                          $">>{sx * 16}|{sz * 16}\n" +
                                          $">>{tx}|{tz}|H{height}|M{maxheight}|GS{grassheightstart}|GSTP{grassheightstop}");
                    for (var by = 0; by < maxheight; ++by)
                    {
                        Block block = new Stone();
                        if (by == 0) block = new Bedrock();
                        else if (by == 1 || by < grassheightstart)
                            block = new Stone();
                        else if (by >= grassheightstart && by < grassheightstop)
                            block = new Dirt();
                        else if (by == grassheightstop) block = new Grass();
                        else
                        {
                            continue;
                        }
                        chunk.SetBlock(bx, by, bz, block);
                    }

                    chunk.SetHeight(bx, bz, (short) maxheight);
                    for (var by2 = maxheight + Dimension == Dimension.Overworld ? 1 : 0; by2 >= 0; --by2)
                        chunk.SetSkyLight(bx, by2, bz, 0);
                    chunk.SetBiome(bx, bz, 1);
                }
            }
        }

        public void PopulateChunk(ChunkColumn chunk)
        {
            var sx = chunk.X;
            var sz = chunk.Z;
            var blockLayers = BlockLayers;
            float tempn = Math.Abs(HeightNoise.GetNoise(sx*16, sz*16));
            float rainn = Math.Abs(HeightNoise.GetNoise(sx*16, sz*16));
            float temp = 2 * tempn;
            
            RePopulateChunk(chunk,temp,rainn);
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