using System;

namespace OpenAPI.WorldGenerator.Generators.Biomes
{
    [Flags]
    public enum BiomeType
    {
        Unknown = 0,
        Ocean = 1,
        River = 2,
        Swamp = 4,
        Beach = 8,
        Land = 16,
        Snowy = 32,
        Cold = 64,
        Coniferous = 128,
        Forest = 256,
        Desert = 512,
        HotHotHot = 1024
    }
}