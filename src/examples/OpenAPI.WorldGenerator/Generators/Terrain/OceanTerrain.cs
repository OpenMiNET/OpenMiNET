namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class OceanTerrain : TerrainBase
    {
        public override float GenerateNoise(OverworldGeneratorV2 rtgWorld, int x, int y, float border, float river)
        {
            return TerrainOcean(x, y, rtgWorld, river, 50f);
        }
    }
}