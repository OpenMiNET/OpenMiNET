using System;
using System.Numerics;

namespace OpenAPI.WorldGenerator.Utils.Noise.Cellular
{
    public class VoronoiResult
    {
        public double ShortestDistance { get; set; } = 32000000.0;
        public double NextDistance { get; set; } = 32000000.0;
        public double ClosestX { get; set; } = 32000000.0;
        public double ClosestZ { get; set; } = 32000000.0;
        
        public double BorderValue => ShortestDistance / NextDistance;
        public double InteriorValue => (NextDistance - ShortestDistance) / NextDistance;

        public VoronoiResult(VoronoiSettings settings)
        {
            ShortestDistance = settings.ShortestDistance;
            NextDistance = settings.NextDistance;
            ClosestX = settings.ClosestX;
            ClosestZ = settings.ClosestZ;
        }
        
        public Vector2 ToLength(Vector2 toMap, float radius) {
            double distance = Distance(toMap, this.ClosestX, this.ClosestZ);
            double xDist = toMap.X - this.ClosestX;
            double zDist = toMap.Y - this.ClosestZ;
            xDist *= radius / distance;
            zDist *= radius / distance;
            return new Vector2((float) (this.ClosestX + xDist), (float) (this.ClosestZ + zDist));
        }

        private static double Distance(Vector2 map, double px, double py)
        {
            px -= map.X;
            py -= map.Y;
            return Math.Sqrt(px * px + py * py);
        }

        public void Evaluate(Vector2[] points, double x, double z) {
            foreach (Vector2 point in points) {
                double distance = Distance(point, x, z);
                if (distance < this.ShortestDistance) {
                    this.NextDistance = this.ShortestDistance;
                    this.ShortestDistance = distance;
                    this.ClosestX = point.X;
                    this.ClosestZ = point.Y;
                }
                else if (distance < this.NextDistance) {
                    this.NextDistance = distance;
                }
            }
        }
    }
}