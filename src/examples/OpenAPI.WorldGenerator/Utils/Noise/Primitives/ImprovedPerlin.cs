using LibNoise;

namespace OpenAPI.WorldGenerator.Utils.Noise.Primitives
{
    /// <summary>
    /// Noise module that outputs 3-dimensional Improved Perlin noise.
    ///
    /// This noise module outputs values that usually range from
    /// -1.0 to +1.0, but there are no guarantees that all output values will
    /// exist within that range.
    /// </summary>
    public class ImprovedPerlin : PrimitiveModule, INoiseModule
    {
        #region Constants

        /// <summary>
        /// 
        /// </summary>
        private const int RandomSize = 256;

        #endregion

        #region Fields

        /// <summary>
        /// Initial permutation table
        /// </summary>
        private static readonly int[] Source = {
            151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142,
            8, 99, 37, 240, 21, 10, 23, 190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203,
            117, 35, 11, 32, 57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165,
            71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41,
            55, 46, 245, 40, 244, 102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89,
            18, 169, 200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250,
            124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189,
            28, 42, 223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9,
            129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 218, 246, 97, 228, 251, 34,
            242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31,
            181, 199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93, 222, 114,
            67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180
        };

        /// <summary>
        /// Stores the random values used to generate the noise
        /// </summary>
        private int[] _random;

        #endregion

        #region Accessors

        /// <summary>
        /// Gets or sets the seed of the perlin noise.
        /// </summary>
        public override int Seed
        {
            get { return base.Seed; }
            set
            {
                if (base.Seed != value)
                {
                    base.Seed = value;
                    Randomize(base.Seed);
                }
            }
        }

        protected int[] Random
        {
            get { return _random; }
            set { _random = value; }
        }

        #endregion

        #region Ctor/Dtor

        /// <summary>
        /// 0-args constructor
        /// </summary>
        public ImprovedPerlin()
            : this(DefaultSeed, DefaultQuality)
        {
        }

        /// <summary>
        /// Create a new ImprovedPerlin with given values
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="quality"></param>
        public ImprovedPerlin(int seed, NoiseQuality quality) : base(seed, quality)
        {
            Randomize(Seed);
        }

        #endregion

        #region Utils

        /// <summary>
        /// Initializes the random values
        /// 
        /// </summary>
        /// <param name="seed">The seed used to generate the random values</param>
        private void Randomize(int seed)
        {
            _random = new int[RandomSize*2];

            if (seed != 0)
            {
                // Shuffle the array using the given seed
                // Unpack the seed into 4 bytes then perform a bitwise XOR operation
                // with each byte
                var F = new byte[4];
                Libnoise.UnpackLittleUint32(seed, ref F);

                for (int i = 0; i < Source.Length; i++)
                {
                    /*
					_random[i] =  (F[0] > 0) ? _source[i] ^ F[0] : _source[i];
					_random[i] =  (F[1] > 0) ? _source[i] ^ F[1] : _random[i];
					_random[i] =  (F[2] > 0) ? _source[i] ^ F[2] : _random[i];
					_random[i] =  (F[3] > 0) ? _source[i] ^ F[3] : _random[i];
					*/

                    _random[i] = Source[i] ^ F[0];
                    _random[i] ^= F[1];
                    _random[i] ^= F[2];
                    _random[i] ^= F[3];

                    _random[i + RandomSize] = _random[i];
                }

#if NOISE_RANDOM_PARANOIA
#warning NOISE_RANDOM_PARANOIA is on

                // Test if Random has unique values, a sorted Random array
                // must have values from 0 to 255 
                var __sorted = new int[RandomSize];
                Array.Copy(Random, __sorted, RandomSize);
                Array.Sort(__sorted);

                for (int _j = 0; _j < RandomSize; _j++)
                {
                    if (_j != __sorted[_j])
                        throw new Exception("Unconsistent random value at " + _j + " : " + __sorted[_j]);
                }
#endif
            }
            else
            {
                for (int i = 0; i < RandomSize; i++)
                    _random[i + RandomSize] = _random[i] = Source[i];
            }
        }

        #endregion

        #region IModule1D Members

        /// <summary>
        /// Generates an output value given the coordinates of the specified input value.
        /// </summary>
        /// <param name="x">The input coordinate on the x-axis.</param>
        /// <returns>The resulting output value.</returns>
        public virtual float GetValue(float x)
        {
            // Fast floor
            int xf = (x > 0.0) ? (int) x : (int) x - 1;

            // Compute the cell coordinates
            int X = xf & 255;

            // Retrieve the decimal part of the cell
            x -= xf;

            // Smooth the curve
            float u = 0.0f;

            switch (Quality)
            {
                case NoiseQuality.Fast:
                    u = x;
                    break;

                case NoiseQuality.Standard:
                    u = Libnoise.SCurve3(x);
                    break;

                case NoiseQuality.Best:
                    u = Libnoise.SCurve5(x);
                    break;
            }

            return Libnoise.Lerp(Grad(_random[X], x), Grad(_random[X + 1], x - 1), u);
        }

        #endregion

        #region IModule2D Members

        /// <summary>
        /// Generates an output value given the coordinates of the specified input value.
        /// </summary>
        /// <param name="x">The input coordinate on the x-axis.</param>
        /// <param name="y">The input coordinate on the y-axis.</param>
        /// <returns>The resulting output value.</returns>
        public virtual float GetValue(float x, float y)
        {
            // Fast floor
            int xf = (x > 0.0) ? (int) x : (int) x - 1;
            int yf = (y > 0.0) ? (int) y : (int) y - 1;

            // Compute the cell coordinates
            int X = xf & 255;
            int Y = yf & 255;

            // Retrieve the decimal part of the cell
            x -= xf;
            x -= yf;

            // Smooth the curve
            float u = 0.0f, v = 0.0f;

            switch (Quality)
            {
                case NoiseQuality.Fast:
                    u = x;
                    v = y;
                    break;

                case NoiseQuality.Standard:
                    u = Libnoise.SCurve3(x);
                    v = Libnoise.SCurve3(y);
                    break;

                case NoiseQuality.Best:
                    u = Libnoise.SCurve5(x);
                    v = Libnoise.SCurve5(y);
                    break;
            }

            // Fetch some randoms values from the table
            int A = _random[X] + Y;
            int B = _random[X + 1] + Y;

            // Interpolate between directions 
            return Libnoise.Lerp(
                Libnoise.Lerp(
                    Grad(_random[A], x, y),
                    Grad(_random[B], x - 1, y),
                    u
                    ),
                Libnoise.Lerp(
                    Grad(_random[A + 1], x, y - 1),
                    Grad(_random[B + 1], x - 1, y - 1),
                    u
                    ),
                v
                );
        }

        #endregion

        #region IModule3D Members

        /// <summary>
        /// Generates an output value given the coordinates of the specified input value.
        /// </summary>
        /// <param name="x">The input coordinate on the x-axis.</param>
        /// <param name="y">The input coordinate on the y-axis.</param>
        /// <param name="z">The input coordinate on the z-axis.</param>
        /// <returns>The resulting output value.</returns>
        public virtual float GetValue(float x, float y, float z)
        {
            // Fast floor
            int xf = (x > 0.0) ? (int) x : (int) x - 1;
            int yf = (y > 0.0) ? (int) y : (int) y - 1;
            int zf = (z > 0.0) ? (int) z : (int) z - 1;

            // Compute the cell coordinates
            // Find unit cube that contains the point
            int X = xf & 255;
            int Y = yf & 255;
            int Z = zf & 255;

            // Retrieve the decimal part of the cell = relative X,Y,Z of point in cube
            x -= xf;
            y -= yf;
            z -= zf;

            // Smooth the curve
            float u = 0.0f, v = 0.0f, w = 0.0f;

            switch (Quality)
            {
                case NoiseQuality.Fast:
                    u = x;
                    v = y;
                    w = z;
                    break;

                case NoiseQuality.Standard:
                    u = Libnoise.SCurve3(x);
                    v = Libnoise.SCurve3(y);
                    w = Libnoise.SCurve3(z);
                    break;

                case NoiseQuality.Best:
                    u = Libnoise.SCurve5(x);
                    v = Libnoise.SCurve5(y);
                    w = Libnoise.SCurve5(z);
                    break;
            }

            // Hash coordinates of the 8 cubes corners
            // Fetch some randoms values from the table
            int a = _random[X] + Y;
            int aa = _random[a] + Z;
            int ab = _random[a + 1] + Z;
            int b = _random[X + 1] + Y;
            int ba = _random[b] + Z;
            int bb = _random[b + 1] + Z;

            // Interpolate between directions
            return
                Libnoise.Lerp(
                    Libnoise.Lerp(
                        Libnoise.Lerp(
                            Grad(_random[aa], x, y, z),
                            Grad(_random[ba], x - 1, y, z),
                            u
                            ),
                        Libnoise.Lerp(
                            Grad(_random[ab], x, y - 1, z),
                            Grad(_random[bb], x - 1, y - 1, z),
                            u
                            ),
                        v
                        ),
                    Libnoise.Lerp(
                        Libnoise.Lerp(
                            Grad(_random[aa + 1], x, y, z - 1),
                            Grad(_random[ba + 1], x - 1, y, z - 1),
                            u
                            ),
                        Libnoise.Lerp(
                            Grad(_random[ab + 1], x, y - 1, z - 1),
                            Grad(_random[bb + 1], x - 1, y - 1, z - 1),
                            u
                            ),
                        v
                        ),
                    w
                    );
        }

        #endregion

        /// <summary>
        /// Modifies the result by adding a directional bias
        /// </summary>
        /// <param name="hash">The random value telling in which direction the bias will occur</param>
        /// <param name="x">The amount of the bias on the X axis</param>
        /// <param name="y">The amount of the bias on the Y axis</param>
        /// <param name="z">The amount of the bias on the Z axis</param>
        /// <returns>The directional bias strength</returns>
        private float Grad(int hash, float x, float y, float z)
        {
            /*
			 * TODO Do this test to improve the method
			 * from http://www.cs.unc.edu/~blloyd/comp238/shader.html
			 * I implemented Perlin's improved noise function . 
			 * In his implementation, he chose the gradient associated with each lattice 
			 * point using a series of if statements. 
			 * The lattice coordinate is hashed to an integer. 
			 * The lower bits of the hash code are used to choose the gradient. 
			 * 
			 * I found it to be slightly faster (and easier) to simply put all the 
			 * possible gradients in a table and do a dot product.
			 * 
			float gradientMagnitude(int hash, float x, float y, float z){
				static float g3[16][3] ={
					{ 1, 1, 0},{-1, 1, 0},{ 1,-1, 0},{-1,-1, 0}, // center of cube to edges
					{ 1, 0, 1},{-1, 0, 1},{ 1, 0,-1},{-1, 0,-1},
					{ 0, 1, 1},{ 0,-1, 1},{ 0, 1,-1},{ 0,-1,-1},
					{ 1, 1, 0},{-1, 1, 0},{ 0,-1, 1},{ 0,-1,-1}  // tetrahedron
				};
				int h = hash & 0xf;
				return x * g3[h][0] + y * g3[h][1] + z * g3[h][2];
			}
			 */
            // Fetch the last 4 bits
            int h = hash & 15;

            // Result table for U
            // ---+------+---+------
            //  0 | 0000 | x |  x
            //  1 | 0001 | x | -x
            //  2 | 0010 | x |  x
            //  3 | 0011 | x | -x
            //  4 | 0100 | x |  x
            //  5 | 0101 | x | -x
            //  6 | 0110 | x |  x
            //  7 | 0111 | x | -x
            //  8 | 1000 | y |  y
            //  9 | 1001 | y | -y
            // 10 | 1010 | y |  y
            // 11 | 1011 | y | -y
            // 12 | 1100 | y |  y
            // 13 | 1101 | y | -y
            // 14 | 1110 | y |  y
            // 15 | 1111 | y | -y

            float u = h < 8 ? x : y;

            // Result table for V
            // ---+------+---+------
            //  0 | 0000 | y |  y
            //  1 | 0001 | y |  y
            //  2 | 0010 | y | -y
            //  3 | 0011 | y | -y
            //  4 | 0100 | z |  z
            //  5 | 0101 | z |  z
            //  6 | 0110 | z | -z
            //  7 | 0111 | z | -z
            //  8 | 1000 | z |  z
            //  9 | 1001 | z |  z
            // 10 | 1010 | z | -z
            // 11 | 1011 | z | -z
            // 12 | 1100 | x |  x
            // 13 | 1101 | z |  z
            // 14 | 1110 | x | -x
            // 15 | 1111 | z | -z

            float v = h < 4 ? y : h == 12 || h == 14 ? x : z;

            // Result table for U+V
            // ---+------+----+----+-------
            //  0 | 0000 |  x |  y |  x + y
            //  1 | 0001 | -x |  y | -x + y
            //  2 | 0010 |  x | -y |  x - y
            //  3 | 0011 | -x | -y | -x - y
            //  4 | 0100 |  x |  z |  x + z
            //  5 | 0101 | -x |  z | -x + z
            //  6 | 0110 |  x | -z |  x - z
            //  7 | 0111 | -x | -z | -x - z
            //  8 | 1000 |  y |  z |  y + z
            //  9 | 1001 | -y |  z | -y + z
            // 10 | 1010 |  y | -z |  y - z
            // 11 | 1011 | -y | -z | -y - z

            // The four last results already exists in the table before
            // They are doubled because you must get a result for all
            // 4-bit combinaisons values.

            // 12 | 1100 |  y |  x |  y + x
            // 13 | 1101 | -y |  z | -y + z
            // 14 | 1110 |  y | -x |  y - x
            // 15 | 1111 | -y | -z | -y - z

            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }

        /// <summary>
        /// Modifies the result by adding a directional bias
        /// </summary>
        /// <param name="hash">The random value telling in which direction the bias will occur</param>
        /// <param name="x">The amount of the bias on the X axis</param>
        /// <param name="y">The amount of the bias on the Y axis</param>
        /// <returns>The directional bias strength</returns>
        private float Grad(int hash, float x, float y)
        {
            // Fetch the last 3 bits
            int h = hash & 3;

            // Result table for U
            // ---+------+---+------
            //  0 | 0000 | x |  x
            //  1 | 0001 | x |  x
            //  2 | 0010 | x | -x
            //  3 | 0011 | x | -x

            float u = (h & 2) == 0 ? x : -x;

            // Result table for V
            // ---+------+---+------
            //  0 | 0000 | y |  y
            //  1 | 0001 | y | -y
            //  2 | 0010 | y |  y
            //  3 | 0011 | y | -y

            float v = (h & 1) == 0 ? y : -y;

            // Result table for U + V
            // ---+------+----+----+--
            //  0 | 0000 |  x |  y |  x + y
            //  1 | 0001 |  x | -y |  x - y
            //  2 | 0010 | -x |  y | -x + y
            //  3 | 0011 | -x | -y | -x - y

            return u + v;
        }

        /// <summary>
        /// Modifies the result by adding a directional bias
        /// </summary>
        /// <param name="hash">The random value telling in which direction the bias will occur</param>
        /// <param name="x">The amount of the bias on the X axis</param>
        /// <returns>The directional bias strength</returns>
        private float Grad(int hash, float x)
        {
            // Result table
            // ---+------+----
            //  0 | 0000 |  x 
            //  1 | 0001 | -x 

            return (hash & 1) == 0 ? x : -x;
        }
    }
}