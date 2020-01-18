namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class BeachTerrain : TerrainBase
    {
        public override float GenerateNoise(OverworldGeneratorV2 rtgWorld, int x, int y, float border, float river)
        {
            return TerrainBeach(x, y, rtgWorld, river, 63f);
        }
    }
}