namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class JungleTerrain : TerrainBase
    {
        public override float GenerateNoise(OverworldGeneratorV2 rtgWorld, int x, int y, float border, float river)
        {
            return TerrainFlatLakes(x, y, rtgWorld, river, 66f);
        }
    }
}