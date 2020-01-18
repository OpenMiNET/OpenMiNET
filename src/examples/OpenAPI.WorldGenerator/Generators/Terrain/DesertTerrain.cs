namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class DesertTerrain : TerrainBase
    {
        public override float GenerateNoise(OverworldGeneratorV2 rtgWorld, int x, int y, float border, float river)
        {
            var settings = rtgWorld.Preset;
            float duneHeight = (MinDuneHeight + settings.SandDuneHeight);

            duneHeight *= (1f + rtgWorld.SimplexInstance(2).GetValue(x / 330f, y / 330f)) / 2f;

            float stPitch = 200f;    // The higher this is, the more smoothly dunes blend with the terrain
            float stFactor = duneHeight;
            float hPitch = 70;    // Dune scale
            float hDivisor = 40;

            return TerrainPolar(x, y, rtgWorld, river, stPitch, stFactor, hPitch, hDivisor, BaseHeight) + GetGroundNoise(x, y, 1f, rtgWorld);
        }
    }
}