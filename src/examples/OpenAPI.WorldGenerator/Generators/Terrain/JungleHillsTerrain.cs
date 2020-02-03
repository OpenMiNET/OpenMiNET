namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class JungleHillsTerrain : TerrainBase
    {
        public float HillStrength { get; set; } = 40f;
        
        public JungleHillsTerrain(float bh, float hs) {

            BaseHeight = bh;
            HillStrength = hs;
        }
        
        public override float GenerateNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river)
        {
            return TerrainHighland(x, y, generator, river, 10f, 68f, HillStrength, BaseHeight - 62f);
        }
    }
}