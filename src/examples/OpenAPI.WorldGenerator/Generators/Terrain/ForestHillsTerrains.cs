namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class ForestHillsTerrain : TerrainBase
    {
        public override float GenerateNoise(OverworldGeneratorV2 rtgWorld, int x, int y, float border, float river)
        {
            return TerrainHighland(x, y, rtgWorld, river, 10f, 68f, 30f, BaseHeight - 62f);
        }
    }
}