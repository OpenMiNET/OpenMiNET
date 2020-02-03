namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class ForestTerrain : TerrainBase
    {
        public override float GenerateNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river)
        {
            GroundNoise = GetGroundNoise(x, y, GroundVariation, generator);

            float m = Hills(x, y, 10f, generator);

            float floNoise = 65f + GroundNoise + m;

            return Riverized(floNoise, river);
        }
    }
}