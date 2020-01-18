using System;
using System.Collections.Generic;
using System.Numerics;
using OpenAPI.Utils;

namespace OpenAPI.WorldGenerator.Utils.Noise.Cellular
{
    public class SpacedCellularNoise
    {
        private static int _totalPoints = 100;
        private static int _pointsPerTorus = 25;
        private static double _minDistanceSq = 0.005d;

        private Dictionary<Vector2, Vector2[]> _cache = new Dictionary<Vector2, Vector2[]>(256);
        private Vector2[] _allPoints;
        private int _xSeed;

        private int _ySeed;

        public SpacedCellularNoise(long xSeed)
        {
            this._xSeed = (int) xSeed;
            this._ySeed = new FastRandom((int) xSeed).Next();
            this._allPoints = new Vector2[_totalPoints];
            this.SetPoints();
        }

        private static double MinimalToroidalDistanceSquared(Vector2 point, Vector2[] existing, int count)
        {
            double result = 1.0;
            for (int i = 0; i < count; i++)
            {
                double distance = ToroidalDistanceSquared(point, existing[i]);
                if (distance < result)
                {
                    result = distance;
                }
            }

            return result;
        }

        private static double ToroidalDistanceSquared(Vector2 first, Vector2 second)
        {
            double xDist = Math.Abs(first.X - second.X);
            if (xDist > 0.5)
            {
                xDist = 1.0 - xDist;
            }

            double yDist = Math.Abs(first.Y - second.Y);
            if (yDist > 0.5)
            {
                yDist = 1.0 - yDist;
            }

            return (xDist * xDist) + (yDist * yDist);
        }

        public VoronoiResult Eval2D(double x, double y)
        {

            // this algorithm places the points about five times more frequently
            // so I'm adjusting the passed values rather than recalibrating all the routings
            x = x / 5.0;
            y = y / 5.0;

            int xInt = (x > 0.0 ? (int) x : (int) x - 1);
            int yInt = (y > 0.0 ? (int) y : (int) y - 1);

            // note the algorithm requires a fresh VoronoiResult.
            VoronoiResult result = new VoronoiResult();

            Vector2 square = new Vector2(xInt, yInt);
            // evaluate the points for that square;
            result.Evaluate(this.AreaPoints(square), x, y);

            // now horizontally adjacent squares as appropriate
            double distance = y - yInt;
            if (distance != result.NextDistance)
            {
                result.Evaluate(this.AreaPoints(new Vector2(xInt, yInt - 1)), x, y);
            }

            distance = x - xInt;
            if (distance != result.NextDistance)
            {
                result.Evaluate(this.AreaPoints(new Vector2(xInt - 1, yInt)), x, y);
            }

            distance = yInt - y + 1.0;
            if (distance != result.NextDistance)
            {
                result.Evaluate(this.AreaPoints(new Vector2(xInt, yInt + 1)), x, y);
            }

            distance = xInt - x + 1.0;
            if (distance != result.NextDistance)
            {
                result.Evaluate(this.AreaPoints(new Vector2(xInt + 1, yInt)), x, y);
            }

            // now diagonally adjacent squares
            distance = Math.Min(y - yInt, x - xInt);
            if (distance != result.NextDistance)
            {
                result.Evaluate(this.AreaPoints(new Vector2(xInt - 1, yInt - 1)), x, y);
            }

            distance = Math.Min(yInt - y + 1.0, x - xInt);
            if (distance != result.NextDistance)
            {
                result.Evaluate(this.AreaPoints(new Vector2(xInt - 1, yInt + 1)), x, y);
            }

            distance = Math.Min(yInt - y + 1.0, xInt - x + 1.0);
            if (distance != result.NextDistance)
            {
                result.Evaluate(this.AreaPoints(new Vector2(xInt + 1, yInt + 1)), x, y);
            }

            distance = Math.Min(y - yInt, xInt - x + 1.0);
            if (distance != result.NextDistance)
            {
                result.Evaluate(this.AreaPoints(new Vector2(xInt + 1, yInt - 1)), x, y);
            }

            return result;
        }
        
        /**
         * Working on a algorithm
         * The algorithm is to create a set of points on a unit torus, none of which are too close.
         * Each unit square gets a random subset of these points
         * On hold since I found the problem was that valueNoise was using too small a divisor and
         * letting points overlap
         */
        private Vector2[] AreaPoints(Vector2 area) {
            Vector2[] ret;
            if (!this._cache.TryGetValue(area, out ret))
            {
                ret = this.GeneratedAreaPoints(area);
            }

            return ret;
           // return ((ret = this.cache.get(area)) != null) ? ret : this.generatedAreaPoints(area);
        }
        
        private Vector2[] GeneratedAreaPoints(Vector2 area) {
            FastRandom random = new FastRandom(area.GetHashCode());
            bool[] used = new bool[_totalPoints];
            Vector2[] result = new Vector2[_pointsPerTorus];
            int index = 0;
            for (int i = 0; i < _pointsPerTorus; i++) {
                int advance = random.Next(_totalPoints);
                for (int j = 0; j < advance; j++) {
                    while (used[index]) {
                        index++;
                        if (index >= _totalPoints) {
                            index = 0;
                        }
                    }
                }
                // add the point, offset to the area
                result[i] = new Vector2(this._allPoints[index].X + area.X, this._allPoints[index].Y + area.Y);
                used[i] = true;
            }
            this._cache[area] = result;
            return result;
        }

        private void SetPoints() {
            // this sets all the points in the master torus.
            FastRandom xRandom = new FastRandom(this._xSeed);
            FastRandom yRandom = new FastRandom(this._ySeed);

            // only bump the iterator if we are storing a value
            for (int i = 0; i < _totalPoints; ) {
                Vector2 newPoint = new Vector2((float) xRandom.NextDouble(), (float) yRandom.NextDouble());
                if (MinimalToroidalDistanceSquared(newPoint, _allPoints, i) >= _minDistanceSq) {
                    _allPoints[i++] = newPoint;
                }
            }
        }
    }
}