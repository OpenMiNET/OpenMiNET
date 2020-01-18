namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class ForestTerrain : TerrainBase
    {
        public override float GenerateNoise(OverworldGeneratorV2 rtgWorld, int x, int y, float border, float river)
        {
            GroundNoise = GetGroundNoise(x, y, GroundVariation, rtgWorld);

            float m = Hills(x, y, 10f, rtgWorld);

            float floNoise = 65f + GroundNoise + m;

            return Riverized(floNoise, river);
        }
    }
}