namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class DesertHillsTerrain : TerrainBase
    {
        private float _start;
        private float _height;
        private float _width;

        public DesertHillsTerrain(float hillStart, float landHeight, float baseHeight, float hillWidth) {

            _start = hillStart;
            _height = landHeight;
            BaseHeight = baseHeight;
            _width = hillWidth;
        }
        
        public override float GenerateNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river)
        {
            return TerrainHighland(x, y, generator, river, _start, _width, _height, BaseHeight - 62f);
        }
    }
}