using OpenAPI.WorldGenerator.Utils.Noise.Cellular;

namespace OpenAPI.WorldGenerator.Generators.Effects
{
    public class VoronoiBorderEffect : HeightEffect
    {
        public float PointWavelength = 0;
        public float Floor = float.MaxValue;
        public float MinimumDivisor = 0;//low divisors can produce excessive rates of change
        
        public override float Added(OverworldGeneratorV2 generator, float x, float y) {
            VoronoiResult points = generator.CellularInstance(1).Eval2D(x / PointWavelength, y / PointWavelength);
            float raise = (float) (points.InteriorValue);
            raise = 1.0f - raise;
            //raise = TerrainBase.blendedHillHeight(raise, floor);
            return raise;
        }
    }
}