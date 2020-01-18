using OpenAPI.WorldGenerator.Utils.Noise.Attributes;

namespace OpenAPI.WorldGenerator.Utils.Noise.Modules
{ 
    /// <summary>
    /// Noise module that outputs three-dimensional "billowy" noise
    /// Hit snoise is also known as Turbulence fBM and generates "billowy" 
    /// noise suitable for clouds and rocks.
    ///
    /// This noise module is nearly identical to SumFractal except
    /// this noise module modifies each octave with an absolute-value
    /// function. Optionally, a scaling factor and a bias addition can be applied 
    /// each octave.
    /// 
    /// The original noise::module::billow has scale of 2 and a bias of -1.
    /// </summary>
    public class BillowNoiseModule : FilterNoise, INoiseModule
    {
        #region Constants

        /// <summary>
        /// Default scale
        /// noise module.
        /// </summary>
        public const float DefaultScale = 1.0f;

        /// <summary>
        /// Default bias
        /// noise module.
        /// </summary>
        public const float DefaultBias = 0.0f;

        #endregion

        #region Fields

        /// <summary>
        /// the bias to apply to the scaled output value from the source module.
        /// </summary>
        protected float PBias = DefaultBias;

        /// <summary>
        /// the scaling factor to apply to the output value from the source module.
        /// </summary>
        protected float PScale = DefaultScale;

        #endregion

        #region Accessors

        /// <summary>
        /// Gets or sets the scale value.
        /// </summary>
        [Modifier]
        public float Scale
        {
            get { return PScale; }
            set { PScale = value; }
        }

        /// <summary>
        /// Gets or sets the bias value.
        /// </summary>
        [Modifier]
        public float Bias
        {
            get { return PBias; }
            set { PBias = value; }
        }

        #endregion

        #region Ctor/Dtor

        #endregion

        #region IModule2D Members

        /// <summary>
        /// Generates an output value given the coordinates of the specified input value.
        /// </summary>
        /// <param name="x">The input coordinate on the x-axis.</param>
        /// <param name="y">The input coordinate on the y-axis.</param>
        /// <returns>The resulting output value.</returns>
        public float GetValue(float x, float y)
        {
            float signal;
            float value;
            int curOctave;

            x *= _frequency;
            y *= _frequency;

            // Initialize value, fBM starts with 0
            value = 0;

            // Inner loop of spectral construction, where the fractal is built

            for (curOctave = 0; curOctave < _octaveCount; curOctave++)
            {
                // Get the coherent-noise value.
                signal = _source.GetValue(x, y)*_spectralWeights[curOctave];

               // if (signal < 0.0f)
                //    signal = -signal;

                // Add the signal to the output value.
                value += (signal*PScale) + PBias;

                // Go to the next octave.
                x *= _lacunarity;
                y *= _lacunarity;
            }

            //take care of remainder in _octaveCount
            float remainder = _octaveCount - (int) _octaveCount;
            if (remainder > 0)
                value += (PScale*remainder*_source.GetValue(x, y)*_spectralWeights[curOctave]) + PBias;

            return value;
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
        public float GetValue(float x, float y, float z)
        {
            int curOctave;

            x *= _frequency;
            y *= _frequency;
            z *= _frequency;

            // Initialize value, fBM starts with 0
            float value = 0;

            // Inner loop of spectral construction, where the fractal is built
            for (curOctave = 0; curOctave < _octaveCount; curOctave++)
            {
                // Get the coherent-noise value.
                float signal = _source.GetValue(x, y, z)*_spectralWeights[curOctave];

                if (signal < 0.0f)
                    signal = -signal;

                // Add the signal to the output value.
                value += (signal*PScale) + PBias;

                // Go to the next octave.
                x *= _lacunarity;
                y *= _lacunarity;
                z *= _lacunarity;
            }

            //take care of remainder in _octaveCount
            float remainder = _octaveCount - (int) _octaveCount;
            if (remainder > 0.0f)
                value += (PScale*remainder*_source.GetValue(x, y, z)*_spectralWeights[curOctave]) + PBias;

            return value;
        }

        #endregion
    }
}