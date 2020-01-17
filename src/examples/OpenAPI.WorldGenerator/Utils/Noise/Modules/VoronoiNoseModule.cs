using System;
using LibNoise;
using OpenAPI.WorldGenerator.Utils.Noise.Attributes;

namespace OpenAPI.WorldGenerator.Utils.Noise.Modules
{
    public class VoronoiNoseModule : FilterNoise, INoiseModule
    {
        public const float DefaultDisplacement = 1.0f;

        public VoronoiNoseModule()
        {
            
        }
        
        private float _displacement = DefaultDisplacement;

        private bool _distance;

        
        /// <summary>
        /// This noise module assigns each Voronoi cell with a random constant
        /// value from a coherent-noise function.  The <i>displacement
        /// value</i> controls the range of random values to assign to each
        /// cell.  The range of random values is +/- the displacement value.
        /// </summary>
        [Modifier]
        public float Displacement
        {
            get { return _displacement; }
            set { _displacement = value; }
        }

        /// <summary>
        /// Applying the distance from the nearest seed point to the output
        /// value causes the points in the Voronoi cells to increase in value
        /// the further away that point is from the nearest seed point.
        /// </summary>
        [Modifier]
        public bool Distance
        {
            get { return _distance; }
            set { _distance = value; }
        }
        
        public float GetValue(float x, float y)
        {
            x *= _frequency;
            y *= _frequency;

            int xInt = (x > 0.0f ? (int)x : (int)x - 1);
            int yInt = (y > 0.0f ? (int)y : (int)y - 1);

            float minDist = 2147483647.0f;
            float xCandidate = 0.0f;
            float yCandidate = 0.0f;

            // Inside each unit cube, there is a seed point at a random position.  Go
            // through each of the nearby cubes until we find a cube with a seed point
            // that is closest to the specified position.
            for (int yCur = yInt - 2; yCur <= yInt + 2; yCur++)
            {
                for (int xCur = xInt - 2; xCur <= xInt + 2; xCur++)
                {
                    // Calculate the position and distance to the seed point inside of
                    // this unit cube.
                    var off = _source.GetValue(xCur, yCur);
                    float xPos = xCur + off;//_source2D.GetValue(xCur, yCur);
                    float yPos = yCur + off;//_source2D.GetValue(xCur, yCur);

                    float xDist = xPos - x;
                    float yDist = yPos - y;
                    float dist = xDist * xDist + yDist * yDist;

                    if (dist < minDist)
                    {
                        // This seed point is closer to any others found so far, so record
                        // this seed point.
                        minDist = dist;
                        xCandidate = xPos;
                        yCandidate = yPos;
                    }
                }
            }

            float value;

            if (_distance)
            {
                // Determine the distance to the nearest seed point.
                float xDist = xCandidate - x;
                float yDist = yCandidate - y;
                value = (MathF.Sqrt(xDist * xDist + yDist * yDist)
                        ) * Libnoise.Sqrt3 - 1.0f;
            }
            else
                value = 0.0f;

            // Return the calculated distance with the displacement value applied.
            return value + (_displacement * _source.GetValue(
                                Libnoise.FastFloor(xCandidate),
                                Libnoise.FastFloor(yCandidate))
                   );
        }
        
        /// <summary>
        /// Generates an output value given the coordinates of the specified input value.
        /// </summary>
        /// <param name="x">The input coordinate on the x-axis.</param>
        /// <param name="y">The input coordinate on the y-axis.</param>
        /// <param name="z">The input coordinate on the z-axis.</param>
        /// <returns>The resulting output value.</returns>
        public float GetValue(float x, float y, float z)
        {
            //TODO This method could be more efficient by caching the seed values.
            x *= _frequency;
            y *= _frequency;
            z *= _frequency;

            int xInt = (x > 0.0f ? (int) x : (int) x - 1);
            int yInt = (y > 0.0f ? (int) y : (int) y - 1);
            int zInt = (z > 0.0f ? (int) z : (int) z - 1);

            float minDist = 2147483647.0f;
            float xCandidate = 0.0f;
            float yCandidate = 0.0f;
            float zCandidate = 0.0f;

            // Inside each unit cube, there is a seed point at a random position.  Go
            // through each of the nearby cubes until we find a cube with a seed point
            // that is closest to the specified position.
            for (int zCur = zInt - 2; zCur <= zInt + 2; zCur++)
            {
                for (int yCur = yInt - 2; yCur <= yInt + 2; yCur++)
                {
                    for (int xCur = xInt - 2; xCur <= xInt + 2; xCur++)
                    {
                        // Calculate the position and distance to the seed point inside of
                        // this unit cube.
                        float xPos = xCur + _source.GetValue(xCur, yCur, zCur);
                        float yPos = yCur + _source.GetValue(xCur, yCur, zCur);
                        float zPos = zCur + _source.GetValue(xCur, yCur, zCur);

                        float xDist = xPos - x;
                        float yDist = yPos - y;
                        float zDist = zPos - z;
                        float dist = xDist*xDist + yDist*yDist + zDist*zDist;

                        if (dist < minDist)
                        {
                            // This seed point is closer to any others found so far, so record
                            // this seed point.
                            minDist = dist;
                            xCandidate = xPos;
                            yCandidate = yPos;
                            zCandidate = zPos;
                        }
                    }
                }
            }

            float value;

            if (_distance)
            {
                // Determine the distance to the nearest seed point.
                float xDist = xCandidate - x;
                float yDist = yCandidate - y;
                float zDist = zCandidate - z;
                value = ((float) Math.Sqrt(xDist*xDist + yDist*yDist + zDist*zDist)
                    )*Libnoise.Sqrt3 - 1.0f;
            }
            else
                value = 0.0f;

            // Return the calculated distance with the displacement value applied.
            return value + (_displacement*_source.GetValue(
                (int) (Math.Floor(xCandidate)),
                (int) (Math.Floor(yCandidate)),
                (int) (Math.Floor(zCandidate)))
                );
        }
    }
}