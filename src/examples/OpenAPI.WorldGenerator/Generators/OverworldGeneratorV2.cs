using System;
using LibNoise;
using MiNET.Utils;
using MiNET.Worlds;
using Newtonsoft.Json;
using OpenAPI.WorldGenerator.Generators.Biomes;
using OpenAPI.WorldGenerator.Generators.Terrain;
using OpenAPI.WorldGenerator.Utils.Noise;
using OpenAPI.WorldGenerator.Utils.Noise.Cellular;
using OpenAPI.WorldGenerator.Utils.Noise.Modules;
using OpenAPI.WorldGenerator.Utils.Noise.Primitives;
using BiomeUtils = OpenAPI.WorldGenerator.Utils.BiomeUtils;

namespace OpenAPI.WorldGenerator.Generators
{
    public class OverworldGeneratorV2 : IWorldGenerator
    {
        public static readonly float ActualRiverProportion = 150f / 1600f;//This value is also used in BiomeAnalyser#riverAdjusted
        public static readonly float RiverFlatteningAddend = ActualRiverProportion / (1f - ActualRiverProportion);
        public static readonly double RiverLargeBendSizeBase = 140D;
        public static readonly double RiverSmallBendSizeBase = 30D;
        public static readonly double RiverSeparationBase = 975D;
        public static readonly double RiverValleyLevelBase = 140D / 450D;
        public static readonly float LakeFrequencyBase = 649.0f;
        public static readonly float LakeShoreLevelBase = 0.035f;
        public static readonly float LakeDepressionLevelBase = 0.15f; // the lakeStrength below which land should start to be lowered
        public static readonly float LakeBendSizeLarge = 80;
        public static readonly float LakeBendSizeMedium = 30;
        public static readonly float LakeBendSizeSmall = 12;
        public static readonly int SimplexInstanceCount = 10;
        public static readonly int CellularInstanceCount = 5;


        public WorldGeneratorPreset Preset { get; }
        public BiomeProvider BiomeProvider { get; set; } 
        
        private readonly SimplexPerlin[] _simplexNoiseInstances = new SimplexPerlin[SimplexInstanceCount];
        private readonly SpacedCellularNoise[] _cellularNoiseInstances = new SpacedCellularNoise[CellularInstanceCount];

        private const int SampleSize = 8;
        private const int SampleArraySize = SampleSize * 2 + 5;
        private readonly float[][] _weightings = new float[SampleArraySize * SampleArraySize][];
        //private readonly int[] _biomeData = new int[SampleArraySize * SampleArraySize];
        
        public OverworldGeneratorV2()
        {
            Preset = new WorldGeneratorPreset();
            Preset.BiomeSize = 1f;
            //Preset = JsonConvert.DeserializeObject<WorldGeneratorPreset>("{\"coordinateScale\":175.0,\"heightScale\":75.0,\"lowerLimitScale\":512.0,\"upperLimitScale\":512.0,\"depthNoiseScaleX\":200.0,\"depthNoiseScaleZ\":200.0,\"depthNoiseScaleExponent\":0.5,\"mainNoiseScaleX\":165.0,\"mainNoiseScaleY\":106.61267,\"mainNoiseScaleZ\":165.0,\"baseSize\":8.267606,\"stretchY\":13.387607,\"biomeDepthWeight\":1.2,\"biomeDepthOffset\":0.2,\"biomeScaleWeight\":3.4084506,\"biomeScaleOffset\":0.0,\"seaLevel\":63,\"useCaves\":true,\"useDungeons\":true,\"dungeonChance\":7,\"useStrongholds\":true,\"useVillages\":true,\"useMineShafts\":true,\"useTemples\":true,\"useMonuments\":true,\"useRavines\":true,\"useWaterLakes\":true,\"waterLakeChance\":49,\"useLavaLakes\":true,\"lavaLakeChance\":80,\"useLavaOceans\":false,\"fixedBiome\":-1,\"biomeSize\":8,\"riverSize\":5,\"dirtSize\":33,\"dirtCount\":10,\"dirtMinHeight\":0,\"dirtMaxHeight\":256,\"gravelSize\":33,\"gravelCount\":8,\"gravelMinHeight\":0,\"gravelMaxHeight\":256,\"graniteSize\":33,\"graniteCount\":10,\"graniteMinHeight\":0,\"graniteMaxHeight\":80,\"dioriteSize\":33,\"dioriteCount\":10,\"dioriteMinHeight\":0,\"dioriteMaxHeight\":80,\"andesiteSize\":33,\"andesiteCount\":10,\"andesiteMinHeight\":0,\"andesiteMaxHeight\":80,\"coalSize\":17,\"coalCount\":20,\"coalMinHeight\":0,\"coalMaxHeight\":128,\"ironSize\":9,\"ironCount\":20,\"ironMinHeight\":0,\"ironMaxHeight\":64,\"goldSize\":9,\"goldCount\":2,\"goldMinHeight\":0,\"goldMaxHeight\":32,\"redstoneSize\":8,\"redstoneCount\":8,\"redstoneMinHeight\":0,\"redstoneMaxHeight\":16,\"diamondSize\":8,\"diamondCount\":1,\"diamondMinHeight\":0,\"diamondMaxHeight\":16,\"lapisSize\":7,\"lapisCount\":1,\"lapisCenterHeight\":16,\"lapisSpread\":16}");
           // Preset = JsonConvert.DeserializeObject<WorldGeneratorPreset>(
            //          "{\"useCaves\":true,\"useStrongholds\":true,\"useVillages\":true,\"useMineShafts\":true,\"useTemples\":true,\"useRavines\":true,\"useMonuments\":true,\"useMansions\":true,\"useLavaOceans\":false,\"useWaterLakes\":true,\"useLavaLakes\":true,\"useDungeons\":true,\"fixedBiome\":-3,\"biomeSize\":4,\"seaLevel\":63,\"riverSize\":4,\"waterLakeChance\":4,\"lavaLakeChance\":80,\"dungeonChance\":8,\"dirtSize\":33,\"dirtCount\":10,\"dirtMinHeight\":0,\"dirtMaxHeight\":255,\"gravelSize\":33,\"gravelCount\":8,\"gravelMinHeight\":0,\"gravelMaxHeight\":255,\"graniteSize\":33,\"graniteCount\":10,\"graniteMinHeight\":0,\"graniteMaxHeight\":80,\"dioriteSize\":33,\"dioriteCount\":10,\"dioriteMinHeight\":0,\"dioriteMaxHeight\":80,\"andesiteSize\":33,\"andesiteCount\":10,\"andesiteMinHeight\":0,\"andesiteMaxHeight\":80,\"coalSize\":17,\"coalCount\":20,\"coalMinHeight\":0,\"coalMaxHeight\":128,\"ironSize\":9,\"ironCount\":20,\"ironMinHeight\":0,\"ironMaxHeight\":64,\"goldSize\":9,\"goldCount\":2,\"goldMinHeight\":0,\"goldMaxHeight\":32,\"redstoneSize\":8,\"redstoneCount\":8,\"redstoneMinHeight\":0,\"redstoneMaxHeight\":16,\"diamondSize\":8,\"diamondCount\":1,\"diamondMinHeight\":0,\"diamondMaxHeight\":16,\"lapisSize\":7,\"lapisCount\":1,\"lapisMinHeight\":0,\"lapisMaxHeight\":32,\"coordinateScale\":684,\"heightScale\":684,\"mainNoiseScaleX\":80,\"mainNoiseScaleY\":160,\"mainNoiseScaleZ\":80,\"depthNoiseScaleX\":200,\"depthNoiseScaleZ\":200,\"depthNoiseScaleExponent\":0.5,\"biomeDepthWeight\":1,\"biomeDepthOffset\":0,\"biomeScaleWeight\":1,\"biomeScaleOffset\":1,\"lowerLimitScale\":512,\"upperLimitScale\":512,\"baseSize\":8.5,\"stretchY\":12,\"lapisCenterHeight\":16,\"lapisSpread\":16}");;
                
            int seed = 345973947;

            InitBiomeProviders(seed);
            
            BiomeUtils.FixMinMaxHeight();
            
            for (int i = 0; i < SimplexInstanceCount; i++) {
                this._simplexNoiseInstances[i] = new SimplexPerlin(seed + i, NoiseQuality.Fast);
            }
            for (int i = 0; i < CellularInstanceCount; i++) {
                this._cellularNoiseInstances[i] = new SpacedCellularNoise(seed + i);
            }

            for (int i = 0; i < _weightings.Length; i++)
            {
                _weightings[i] = new float[256];
            }
            
            for (int x = 0; x < 16; x++) {
                for (int z = 0; z < 16; z++) {
                    float limit = MathF.Pow((56f * 56f), 0.7F);
                    for (int mapX = 0; mapX < SampleArraySize; mapX++) {
                        for (int mapZ = 0; mapZ < SampleArraySize; mapZ++) {
                            float xDist = (x - (mapX - SampleSize) * 8);
                            float zDist = (z - (mapZ - SampleSize) * 8);
                            float distanceSquared = xDist * xDist + zDist * zDist;
                            float distance = MathF.Pow(distanceSquared, 0.7F);
                            float weight = 1f - distance / limit;
                            if (weight < 0) {
                                weight = 0;
                            }
                            
                            _weightings[mapX * SampleArraySize + mapZ][x * 16 + z] = weight;
                        }
                    }
                }
            }
        }

        public double RiverSeperation => RiverSeparationBase / Preset.RiverFrequency;
        public double RiverSmallBendSize => RiverSmallBendSizeBase * Preset.RiverBendMult;
        public double RiverLargeBendSize => RiverLargeBendSizeBase * Preset.RiverBendMult;
        public double RiverValleyLevel => RiverValleyLevelBase * Preset.RiverSizeMult * Preset.RiverFrequency;

        public float LakeFrequency => LakeFrequencyBase * Preset.RTGlakeFreqMult;
        public float LakeShoreLevel => LakeShoreLevelBase * Preset.RTGlakeFreqMult * Preset.RTGlakeSizeMult;

        public float LakeDepressionLevel => LakeDepressionLevelBase * Preset.RTGlakeFreqMult *
                                            Preset.RTGlakeSizeMult;
        
        public SimplexPerlin SimplexInstance(int index) {
            if (index >= this._simplexNoiseInstances.Length) {
                index = 0;
            }
            return this._simplexNoiseInstances[index];
        }
        
        public SpacedCellularNoise CellularInstance(int index) {
            if (index >= this._cellularNoiseInstances.Length) {
                index = 0;
            }
            return this._cellularNoiseInstances[index];
        }

        public void Initialize()
        {
            
        }
        
        private void InitBiomeProviders(int seed)
        {
            var biomeScale = 32f * Preset.BiomeSize;

            SimplexPerlin temperaturePerlin  = new SimplexPerlin(seed + seed * seed, NoiseQuality.Fast);
            INoiseModule temperatureNoise = new OctaveNoise(temperaturePerlin, 4)
            {
                Amplitude = 3f,
                Frequency = 0.535f
            };

            temperatureNoise = new ScaledNoiseModule(temperatureNoise)
            {
                ScaleX = 1f / biomeScale,
                ScaleZ = 1f / biomeScale,
                ScaleY = 1f / biomeScale
            };

            temperatureNoise = new VoronoiNoseModule()
            {
                Primitive = temperatureNoise,
                Distance = false,
                Frequency = 0.2325f,
            //    SpectralExponent = 0.25f
            };
            
            INoiseModule rainNoise = new SimplexPerlin(seed - seed * seed, NoiseQuality.Fast);
            rainNoise = new OctaveNoise(rainNoise, 4)
            {
                Amplitude = 3f,
                Frequency = 0.345f
            };
            
            rainNoise = new ScaledNoiseModule(rainNoise)
            {
                ScaleX = 1f / biomeScale,
                ScaleZ = 1f / biomeScale,
                ScaleY = 1f / biomeScale
            };

            rainNoise = new VoronoiNoseModule()
            {
                Primitive = rainNoise,
                Distance = false,
                Frequency = 0.25f,
               // SpectralExponent = 0.25f
            };

            BiomeProvider = new BiomeProvider()
            {
                RainfallProvider = rainNoise,
                TemperatureProvider = temperatureNoise
            };
        }

        public bool ApplyBlocks { get; set; } = true;
        public ChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates)
        {
            ChunkLandscape landscape = new ChunkLandscape();
           var biomes = CalculateBiomes(chunkCoordinates, landscape);

            ChunkColumn column = new ChunkColumn()
            {
                 x = chunkCoordinates.X,
                 z = chunkCoordinates.Z
            };

            var heightMap = landscape.Noise;

            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    var biome = biomes[NoiseMap.GetIndex(x, z)];

                    byte biomeId = (byte) biome.Id;
                    if (ApplyBlocks)
                    {
                        var h = (int)heightMap[NoiseMap.GetIndex(x, z)];
                        
                        var height = h;
                        for (int y = 0; (y <= height || y <= Preset.SeaLevel) && y < 255; y++)
                        {
                            if (y > height)
                            {
                                if (y < Preset.SeaLevel)
                                {
                                    column.SetBlock(x, y, z, 8);
                                }
                            }
                            else if (y == height)
                            {
                                column.SetBlock(x, y, z, biome.SurfaceBlock);
                                column.SetMetadata(x, y, z, biome.SurfaceMetadata);
                                
                                column.SetBlock(x, y - 1, z, biome.SoilBlock);
                                column.SetMetadata(x, y -1, z, biome.SoilMetadata);
                            }
                            else if (y < height)
                            {
                                column.SetBlock(x, y, z, 1);
                            }
                        }
                        
                        
                        column.SetHeight(x, z, (short) height);
                    }

                    column.SetBiome(x, z, biomeId);
                }
            }

            return column;
        }

        public float MaxHeight { get; set; } = -1f;
        public float MinHeight { get; set; } = 0f;
        
        private BiomeBase[] CalculateBiomes(ChunkCoordinates coordinates, ChunkLandscape landscape)
        {
            int worldX = coordinates.X * 16;
            int worldZ = coordinates.Z * 16;
            float[] weightedBiomes = new float[256];
            var biomeData = new int[SampleArraySize * SampleArraySize];
            
            for (int x = -SampleSize; x < SampleSize + 5; x++) {
                for (int z = -SampleSize; z < SampleSize + 5; z++) {
                    biomeData[(x + SampleSize) * SampleArraySize + (z + SampleSize)] = (BiomeProvider.GetBiome(worldX + ((x * 8)), worldZ + ((z * 8))).Id);
                }
            }
            
            for (int x = 0; x < 16; x++) {
                for (int z = 0; z < 16; z++)
                {
                    int index = NoiseMap.GetIndex(x, z);
                    
                    float totalWeight = 0;
                    for (int mapX = 0; mapX < SampleArraySize; mapX++) {
                        for (int mapZ = 0; mapZ < SampleArraySize; mapZ++) {
                            float weight = _weightings[mapX * SampleArraySize + mapZ][x * 16 + z];
                            if (weight > 0) {
                                totalWeight += weight;
                                weightedBiomes[biomeData[mapX * SampleArraySize + mapZ]] += weight;
                            }
                        }
                    }

                    // normalize biome weights
                    for (int biomeIndex = 0; biomeIndex < weightedBiomes.Length; biomeIndex++) {
                        weightedBiomes[biomeIndex] /= totalWeight;
                    }

                    // combine mesa biomes
                   // mesaCombiner.adjust(weightedBiomes);

                    landscape.Noise[index] = 0f;

                   float river = TerrainBase.GetRiverStrength(new BlockCoordinates(worldX + x, 0, worldZ + z), this);
                   landscape.River[index] = -river;

                    for (int i = 0; i < 256; i++) {

                        if (weightedBiomes[i] > 0f) {

                           landscape.Noise[index] += BiomeProvider.GetBiome(i).RNoise(this, worldX + x, worldZ + z, weightedBiomes[i], river + 1f) * weightedBiomes[i];
                            
                            // 0 for the next column
                            weightedBiomes[i] = 0f;
                        }
                    }

                    //landscape.Biome[index] = BiomeProvider.GetBiome(weightedBiomes[i]);
                }
            }
            
            for (int x = 0; x < 16; x++) {
                for (int z = 0; z < 16; z++) {
                    BlockCoordinates pos = new BlockCoordinates(worldX + (x - 7) * 8 + 4, 0, worldZ + (z - 7) * 8 + 4);
                    landscape.Biome[x * 16 + z] = BiomeProvider.GetBiome(pos.X, pos.Z);
                }
            }

            return landscape.Biome;
        }
    }
}