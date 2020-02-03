namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class SavannaPlateauTerrain : TerrainBase
    {
        private bool booRiver;
        private float[] height;
        private int heightLength;
        private float strength;
        private float smooth;
        private float cWidth;
        private float cHeigth;
        private float cStrength;
        private float baseVal;
            
        public SavannaPlateauTerrain(bool riverGen, float heightStrength, float canyonWidth, float canyonHeight, float canyonStrength, float baseHeight)
        {
            booRiver = true;
            /*    Values come in pairs per layer. First is how high to step up.
             * 	Second is a value between 0 and 1, signifying when to step up.
             */
            height = new float[]{12.0f, 0.5f, 6f, 0.7f};
            strength = heightStrength;
            heightLength = height.Length;
        }
        
        public override float GenerateNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river)
        {
            return TerrainPlateau(x, y, generator, river, height, border, strength, heightLength, 50f, true);
        }
    }
}