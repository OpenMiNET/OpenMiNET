namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class BeachTerrain : TerrainBase
    {
        public override float GenerateNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river)
        {
            return TerrainBeach(x, y, generator, river, 63f);
        }
    }
}