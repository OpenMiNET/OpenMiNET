using System.Numerics;
using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Effects
{
    public class VoronoiPlateauEffect : HeightEffect
    {
        public float PointWavelength = 200;
        public float MinimumDivisor = 0;//low divisors can produce excessive rates of change
        public float AdjustmentRadius = 3f;

        
        public override float Added(OverworldGeneratorV2 generator, float x, float y)
        {
            var evaluateAt = new Vector2(x / PointWavelength, y / PointWavelength);
            var points = generator.CellularInstance(1).Eval2D(evaluateAt.X, evaluateAt.Y);
            float raise = (float) (points.InteriorValue);
            // now we're going to get an adjustment value which will be the same
            // for all points on a given vector from the voronoi basin center
            var adjustAt = points.ToLength(evaluateAt, AdjustmentRadius);
            float multiplier = 1.3f;
            float noZeros = 0.1f;
            float adjustment = (float) generator.CellularInstance(2).Eval2D(adjustAt.X, adjustAt.Y).InteriorValue * multiplier + noZeros;
            float reAdjustment = (float) generator.CellularInstance(3).Eval2D(adjustAt.X, adjustAt.Y).InteriorValue * multiplier + noZeros;
            // 0 to 1 which is currently undesirable so increase to average closer to 1
            adjustment = TerrainBase.BayesianAdjustment(adjustment, reAdjustment);
            raise = TerrainBase.BayesianAdjustment(raise, adjustment);
            return raise;
        }
    }
}