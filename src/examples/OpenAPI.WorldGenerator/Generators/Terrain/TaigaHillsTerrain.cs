namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class TaigaHillsTerrain : TerrainBase
    {
        private float HillStrength { get; set; } = 30f;
        public TaigaHillsTerrain() : this(72f, 30f)
        {
            
        }
        
        public TaigaHillsTerrain(float bh, float hs) {

            BaseHeight = bh;
            HillStrength = hs;
        }
        
        public override float GenerateNoise(OverworldGeneratorV2 rtgWorld, int x, int y, float border, float river)
        {
            return TerrainHighland(x, y, rtgWorld, river, 10f, 68f, HillStrength, BaseHeight - 62f);
        }
    }
}