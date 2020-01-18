namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class BirchForestHillsTerrain : TerrainBase
    {
        private float HillStrength { get; set; } = 35f;
        
        public BirchForestHillsTerrain() {

        }

        public BirchForestHillsTerrain(float bh, float hs) {
            BaseHeight = bh;
            HillStrength = hs;
        }
        
        public override float GenerateNoise(OverworldGeneratorV2 rtgWorld, int x, int y, float border, float river)
        {
            return TerrainHighland(x, y, rtgWorld, river, 10f, 68f, HillStrength, BaseHeight - 62f);
        }
    }
}