namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class DeepOceanTerrain : TerrainBase
    {
        public override float GenerateNoise(OverworldGeneratorV2 rtgWorld, int x, int y, float border, float river)
        {
            return TerrainOcean(x, y, rtgWorld, river, 40f);
        }
    }
}