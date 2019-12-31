using Newtonsoft.Json;

namespace OpenAPI.WorldGenerator.Generators
{
    public class WorldGeneratorPreset
    {
        [JsonProperty("seaLevel")] public long SeaLevel { get; set; } = 63;
        
        [JsonProperty("coordinateScale")] public float CoordinateScale { get; set; } = 684.412f;
        
        [JsonProperty("heightScale")] public float HeightScale { get; set; } = 684.412f;

        [JsonProperty("lowerLimitScale")] public float LowerLimitScale { get; set; } = 512;

        [JsonProperty("upperLimitScale")] public float UpperLimitScale { get; set; } = 512;

        [JsonProperty("depthNoiseScaleX")] public float DepthNoiseScaleX { get; set; } = 200;

        [JsonProperty("depthNoiseScaleZ")] public float DepthNoiseScaleZ { get; set; } = 200;

        [JsonProperty("depthNoiseScaleExponent")] public float DepthNoiseScaleExponent { get; set; } = 0.5f;

        [JsonProperty("mainNoiseScaleX")] public float MainNoiseScaleX { get; set; } = 80f;

        [JsonProperty("mainNoiseScaleY")] public float MainNoiseScaleY { get; set; } = 160f;

        [JsonProperty("mainNoiseScaleZ")] public float MainNoiseScaleZ { get; set; } = 80f;

        [JsonProperty("baseSize")] public float BaseSize { get; set; } = 8.5f;

        [JsonProperty("stretchY")] public float StretchY { get; set; } = 12f;

        [JsonProperty("biomeDepthWeight")] public float BiomeDepthWeight { get; set; } = 1f;

        [JsonProperty("biomeDepthOffset")] public float BiomeDepthOffset { get; set; } = 0f;

        [JsonProperty("biomeScaleWeight")] public float BiomeScaleWeight { get; set; } = 1f;

        [JsonProperty("biomeScaleOffset")] public float BiomeScaleOffset { get; set; } = 0f;

        [JsonProperty("useCaves")]
        public bool UseCaves { get; set; }

        [JsonProperty("useDungeons")]
        public bool UseDungeons { get; set; }

        [JsonProperty("dungeonChance")]
        public long DungeonChance { get; set; }

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

        [JsonProperty("useWaterLakes")]
        public bool UseWaterLakes { get; set; }

        [JsonProperty("waterLakeChance")]
        public long WaterLakeChance { get; set; }

        [JsonProperty("useLavaLakes")]
        public bool UseLavaLakes { get; set; }

        [JsonProperty("lavaLakeChance")]
        public long LavaLakeChance { get; set; }

        [JsonProperty("useLavaOceans")]
        public bool UseLavaOceans { get; set; }

        [JsonProperty("fixedBiome")]
        public long FixedBiome { get; set; }

        [JsonProperty("biomeSize")]
        public long BiomeSize { get; set; }

        [JsonProperty("riverSize")]
        public long RiverSize { get; set; }

        [JsonProperty("dirtSize")]
        public long DirtSize { get; set; }

        [JsonProperty("dirtCount")]
        public long DirtCount { get; set; }

        [JsonProperty("dirtMinHeight")]
        public long DirtMinHeight { get; set; }

        [JsonProperty("dirtMaxHeight")]
        public long DirtMaxHeight { get; set; }

        [JsonProperty("gravelSize")]
        public long GravelSize { get; set; }

        [JsonProperty("gravelCount")]
        public long GravelCount { get; set; }

        [JsonProperty("gravelMinHeight")]
        public long GravelMinHeight { get; set; }

        [JsonProperty("gravelMaxHeight")]
        public long GravelMaxHeight { get; set; }

        [JsonProperty("graniteSize")]
        public long GraniteSize { get; set; }

        [JsonProperty("graniteCount")]
        public long GraniteCount { get; set; }

        [JsonProperty("graniteMinHeight")]
        public long GraniteMinHeight { get; set; }

        [JsonProperty("graniteMaxHeight")]
        public long GraniteMaxHeight { get; set; }

        [JsonProperty("dioriteSize")]
        public long DioriteSize { get; set; }

        [JsonProperty("dioriteCount")]
        public long DioriteCount { get; set; }

        [JsonProperty("dioriteMinHeight")]
        public long DioriteMinHeight { get; set; }

        [JsonProperty("dioriteMaxHeight")]
        public long DioriteMaxHeight { get; set; }

        [JsonProperty("andesiteSize")]
        public long AndesiteSize { get; set; }

        [JsonProperty("andesiteCount")]
        public long AndesiteCount { get; set; }

        [JsonProperty("andesiteMinHeight")]
        public long AndesiteMinHeight { get; set; }

        [JsonProperty("andesiteMaxHeight")]
        public long AndesiteMaxHeight { get; set; }

        [JsonProperty("coalSize")]
        public long CoalSize { get; set; }

        [JsonProperty("coalCount")]
        public long CoalCount { get; set; }

        [JsonProperty("coalMinHeight")]
        public long CoalMinHeight { get; set; }

        [JsonProperty("coalMaxHeight")]
        public long CoalMaxHeight { get; set; }

        [JsonProperty("ironSize")]
        public long IronSize { get; set; }

        [JsonProperty("ironCount")]
        public long IronCount { get; set; }

        [JsonProperty("ironMinHeight")]
        public long IronMinHeight { get; set; }

        [JsonProperty("ironMaxHeight")]
        public long IronMaxHeight { get; set; }

        [JsonProperty("goldSize")]
        public long GoldSize { get; set; }

        [JsonProperty("goldCount")]
        public long GoldCount { get; set; }

        [JsonProperty("goldMinHeight")]
        public long GoldMinHeight { get; set; }

        [JsonProperty("goldMaxHeight")]
        public long GoldMaxHeight { get; set; }

        [JsonProperty("redstoneSize")]
        public long RedstoneSize { get; set; }

        [JsonProperty("redstoneCount")]
        public long RedstoneCount { get; set; }

        [JsonProperty("redstoneMinHeight")]
        public long RedstoneMinHeight { get; set; }

        [JsonProperty("redstoneMaxHeight")]
        public long RedstoneMaxHeight { get; set; }

        [JsonProperty("diamondSize")]
        public long DiamondSize { get; set; }

        [JsonProperty("diamondCount")]
        public long DiamondCount { get; set; }

        [JsonProperty("diamondMinHeight")]
        public long DiamondMinHeight { get; set; }

        [JsonProperty("diamondMaxHeight")]
        public long DiamondMaxHeight { get; set; }

        [JsonProperty("lapisSize")]
        public long LapisSize { get; set; }

        [JsonProperty("lapisCount")]
        public long LapisCount { get; set; }

        [JsonProperty("lapisCenterHeight")]
        public long LapisCenterHeight { get; set; }

        [JsonProperty("lapisSpread")]
        public long LapisSpread { get; set; }
    }
}