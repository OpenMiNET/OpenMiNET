using Newtonsoft.Json;

namespace OpenAPI.WorldGenerator.Generators
{
    public class WorldGeneratorPreset
    {
        [JsonProperty("coordinateScale")] public float CoordinateScale { get; set; } = 684f;

        [JsonProperty("heightScale")] public float HeightScale { get; set; } = 684f;


        [JsonProperty("lowerLimitScale")] public float LowerLimitScale { get; set; } = 512f;

        [JsonProperty("upperLimitScale")] public float UpperLimitScale { get; set; } = 512f;


        [JsonProperty("depthNoiseScaleX")] public float DepthNoiseScaleX { get; set; } = 200f;

        [JsonProperty("depthNoiseScaleZ")] public float DepthNoiseScaleZ { get; set; } = 200f;

        [JsonProperty("depthNoiseScaleExponent")] public float DepthNoiseScaleExponent { get; set; } = 0.5f;


        [JsonProperty("mainNoiseScaleX")] public float MainNoiseScaleX { get; set; } = 80f;

        [JsonProperty("mainNoiseScaleY")] public float MainNoiseScaleY { get; set; } = 160f;

        [JsonProperty("mainNoiseScaleZ")] public float MainNoiseScaleZ { get; set; } = 80f;


        [JsonProperty("baseSize")] public float BaseSize { get; set; } = 8.5f;

        [JsonProperty("stretchY")] public float StretchY { get; set; } = 12f;


        [JsonProperty("biomeDepthWeight")] public float BiomeDepthWeight { get; set; } = 1f;

        [JsonProperty("biomeDepthOffset")] public float BiomeDepthOffset { get; set; } = 0f;

        [JsonProperty("biomeScaleWeight")] public float BiomeScaleWeight { get; set; } = 1f;

        [JsonProperty("biomeScaleOffset")] public float BiomeScaleOffset { get; set; } = 1f;


        [JsonProperty("seaLevel")] public float SeaLevel { get; set; } = 63;


        [JsonProperty("useCaves")] public bool UseCaves { get; set; } = true;

        [JsonProperty("useDungeons")]
        public bool UseDungeons { get; set; }

        [JsonProperty("dungeonChance")]
        public float DungeonChance { get; set; }

        [JsonProperty("useStrongholds")]
        public bool UseStrongholds { get; set; }

        [JsonProperty("useVillages")]
        public bool UseVillages { get; set; }

        [JsonProperty("useMineShafts")]
        public bool UseMineShafts { get; set; }

        [JsonProperty("useTemples")]
        public bool UseTemples { get; set; }

        [JsonProperty("useRavines")]
        public bool UseRavines { get; set; }

        [JsonProperty("useWaterLakes")] public bool UseWaterLakes { get; set; } = true;

        [JsonProperty("waterLakeChance")] public float WaterLakeChance { get; set; } = 4f;

        [JsonProperty("useLavaLakes")] public bool UseLavaLakes { get; set; } = false;

        [JsonProperty("lavaLakeChance")] public float LavaLakeChance { get; set; } = 80;

        [JsonProperty("useLavaOceans")] public bool UseLavaOceans { get; set; } = false;

        [JsonProperty("fixedBiome")] public float FixedBiome { get; set; } = -3f;

        [JsonProperty("biomeSize")] public float BiomeSize { get; set; } = 4f;

        [JsonProperty("riverSize")] public float RiverSize { get; set; } = 4f;

        [JsonProperty("dirtSize")]
        public float DirtSize { get; set; }

        [JsonProperty("dirtCount")]
        public float DirtCount { get; set; }

        [JsonProperty("dirtMinHeight")]
        public float DirtMinHeight { get; set; }

        [JsonProperty("dirtMaxHeight")]
        public float DirtMaxHeight { get; set; }

        [JsonProperty("gravelSize")]
        public float GravelSize { get; set; }

        [JsonProperty("gravelCount")]
        public float GravelCount { get; set; }

        [JsonProperty("gravelMinHeight")]
        public float GravelMinHeight { get; set; }

        [JsonProperty("gravelMaxHeight")]
        public float GravelMaxHeight { get; set; }

        [JsonProperty("graniteSize")]
        public float GraniteSize { get; set; }

        [JsonProperty("graniteCount")]
        public float GraniteCount { get; set; }

        [JsonProperty("graniteMinHeight")]
        public float GraniteMinHeight { get; set; }

        [JsonProperty("graniteMaxHeight")]
        public float GraniteMaxHeight { get; set; }

        [JsonProperty("dioriteSize")]
        public float DioriteSize { get; set; }

        [JsonProperty("dioriteCount")]
        public float DioriteCount { get; set; }

        [JsonProperty("dioriteMinHeight")]
        public float DioriteMinHeight { get; set; }

        [JsonProperty("dioriteMaxHeight")]
        public float DioriteMaxHeight { get; set; }

        [JsonProperty("andesiteSize")]
        public float AndesiteSize { get; set; }

        [JsonProperty("andesiteCount")]
        public float AndesiteCount { get; set; }

        [JsonProperty("andesiteMinHeight")]
        public float AndesiteMinHeight { get; set; }

        [JsonProperty("andesiteMaxHeight")]
        public float AndesiteMaxHeight { get; set; }

        [JsonProperty("coalSize")]
        public float CoalSize { get; set; }

        [JsonProperty("coalCount")]
        public float CoalCount { get; set; }

        [JsonProperty("coalMinHeight")]
        public float CoalMinHeight { get; set; }

        [JsonProperty("coalMaxHeight")]
        public float CoalMaxHeight { get; set; }

        [JsonProperty("ironSize")]
        public float IronSize { get; set; }

        [JsonProperty("ironCount")]
        public float IronCount { get; set; }

        [JsonProperty("ironMinHeight")]
        public float IronMinHeight { get; set; }

        [JsonProperty("ironMaxHeight")]
        public float IronMaxHeight { get; set; }

        [JsonProperty("goldSize")]
        public float GoldSize { get; set; }

        [JsonProperty("goldCount")]
        public float GoldCount { get; set; }

        [JsonProperty("goldMinHeight")]
        public float GoldMinHeight { get; set; }

        [JsonProperty("goldMaxHeight")]
        public float GoldMaxHeight { get; set; }

        [JsonProperty("redstoneSize")]
        public float RedstoneSize { get; set; }

        [JsonProperty("redstoneCount")]
        public float RedstoneCount { get; set; }

        [JsonProperty("redstoneMinHeight")]
        public float RedstoneMinHeight { get; set; }

        [JsonProperty("redstoneMaxHeight")]
        public float RedstoneMaxHeight { get; set; }

        [JsonProperty("diamondSize")]
        public float DiamondSize { get; set; }

        [JsonProperty("diamondCount")]
        public float DiamondCount { get; set; }

        [JsonProperty("diamondMinHeight")]
        public float DiamondMinHeight { get; set; }

        [JsonProperty("diamondMaxHeight")]
        public float DiamondMaxHeight { get; set; }

        [JsonProperty("lapisSize")]
        public float LapisSize { get; set; }

        [JsonProperty("lapisCount")]
        public float LapisCount { get; set; }

        [JsonProperty("lapisCenterHeight")]
        public float LapisCenterHeight { get; set; }

        [JsonProperty("lapisSpread")]
        public float LapisSpread { get; set; }

        public float RiverFrequency { get; set; } = 1.0f;
        public float RiverBendMult { get; set; } = 1.0f;
        public float RiverSizeMult { get; set; } = 1.0f;

        public float RTGlakeSizeMult { get; set; } = 1f;      // RTG
        public float RTGlakeFreqMult { get; set; } = 1f;       // RTG
        public float RTGlakeShoreBend { get; set; } = 1f; // RTG

        public float SandDuneHeight { get; set; } = 4f;
    }
}