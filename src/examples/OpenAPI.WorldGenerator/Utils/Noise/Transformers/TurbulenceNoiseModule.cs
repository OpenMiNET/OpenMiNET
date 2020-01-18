namespace OpenAPI.WorldGenerator.Utils.Noise.Transformers
{
    /// <summary>
    /// Noise module that randomly displaces the input value before
    /// returning the output value from a source module.
    ///
    /// Turbulence is the pseudo-random displacement of the input value.
    /// The GetValue() method randomly displaces the ( x, y, z )
    /// coordinates of the input value before retrieving the output value from
    /// the source module.
    ///
    /// The power of the turbulence determines the scaling factor that is
    /// applied to the displacement amount.  To specify the power, use the
    /// Power property.
    ///
    /// Use of this noise module may require some trial and error.  Assuming
    /// that you are using a generator module as the source module, you
    /// should first set the power to the reciprocal of the frequency.
    /// 
    ///
    /// Displacing the input values result in more realistic terrain and
    /// textures.  If you are generating elevations for terrain height maps,
    /// you can use this noise module to produce more realistic mountain
    /// ranges or terrain features that look like flowing lava rock.  If you
    /// are generating values for textures, you can use this noise module to
    /// produce realistic marble-like or "oily" textures.
    ///
    /// Internally, there are three noise modules
    /// that displace the input value; one for the x, one for the y,
    /// and one for the z coordinate.
    /// </summary>
    public class TurbulenceNoiseModule : TransformerModule, INoiseModule
    {
           #region Constants

        /// <summary>
        /// Default power for the Turbulence noise module
        /// </summary>
        public const float DEFAULT_POWER = 1.0f;

        #endregion

        #region Fields

        /// <summary>
        /// The power (scale) of the displacement.
        /// </summary>
        protected float _power = DEFAULT_POWER;

        /// <summary>
        /// The source input module
        /// </summary>
        protected INoiseModule _sourceModule;

        /// <summary>
        /// Noise module that displaces the x coordinate.
        /// </summary>
        protected INoiseModule _xDistortModule;

        /// <summary>
        /// Noise module that displaces the y coordinate.
        /// </summary>
        protected INoiseModule _yDistortModule;

        /// <summary>
        /// Noise module that displaces the z coordinate.
        /// </summary>
        protected INoiseModule _zDistortModule;

        #endregion

        #region Accessors

        /// <summary>
        /// Gets or sets the source module
        /// </summary>
        public INoiseModule SourceModule
        {
            get { return _sourceModule; }
            set { _sourceModule = value; }
        }

        /// <summary>
        /// Gets or sets the noise module that displaces the x coordinate.
        /// </summary>
        public INoiseModule XDistortModule
        {
            get { return _xDistortModule; }
            set { _xDistortModule = value; }
        }

        /// <summary>
        /// Gets or sets the noise module that displaces the y coordinate.
        /// </summary>
        public INoiseModule YDistortModule
        {
            get { return _yDistortModule; }
            set { _yDistortModule = value; }
        }

        /// <summary>
        /// Gets or sets the noise module that displaces the z coordinate.
        /// </summary>
        public INoiseModule ZDistortModule
        {
            get { return _zDistortModule; }
            set { _zDistortModule = value; }
        }

        /// <summary>
        /// Returns the power of the turbulence.
        ///
        /// The power of the turbulence determines the scaling factor that is
        /// applied to the displacement amount.
        /// </summary>
        public float Power
        {
            get { return _power; }
            set { _power = value; }
        }

        #endregion

        #region Ctor/Dtor

        /// <summary>
        /// Create a new noise module with default values
        /// </summary>
        public TurbulenceNoiseModule()
        {
            _power = DEFAULT_POWER;
        }


        /// <summary>
        /// Create a new noise module with the given values
        /// </summary>
        /// <param name="source">the source module</param>
        public TurbulenceNoiseModule(INoiseModule source) : this()
        {
            _sourceModule = source;
        }


        /// <summary>
        /// Create a new noise module with the given values.
        /// </summary>
        /// <param name="source">the source module</param>
        /// <param name="xDistortModule">the noise module that displaces the x coordinate</param>
        /// <param name="yDistortModule">the noise module that displaces the y coordinate</param>
        /// <param name="zDistortModule">the noise module that displaces the z coordinate</param>
        /// <param name="power">the power of the turbulence</param>
        public TurbulenceNoiseModule(INoiseModule source, INoiseModule xDistortModule, INoiseModule yDistortModule,
            INoiseModule zDistortModule,
            float power)
        {
            _sourceModule = source;

            _xDistortModule = xDistortModule;
            _yDistortModule = yDistortModule;
            _zDistortModule = zDistortModule;

            _power = power;
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
            // Get the values from the three Perlin noise modules and
            // add each value to each coordinate of the input value.  There are also
            // some offsets added to the coordinates of the input values.  This prevents
            // the distortion modules from returning zero if the (x, y, z) coordinates,
            // when multiplied by the frequency, are near an integer boundary.  This is
            // due to a property of gradient coherent noise, which returns zero at
            // integer boundaries.
            float x0, y0, z0;
            float x1, y1, z1;
            float x2, y2, z2;

            x0 = x + (12414.0f/65536.0f);
            y0 = y + (65124.0f/65536.0f);
            z0 = z + (31337.0f/65536.0f);

            x1 = x + (26519.0f/65536.0f);
            y1 = y + (18128.0f/65536.0f);
            z1 = z + (60493.0f/65536.0f);

            x2 = x + (53820.0f/65536.0f);
            y2 = y + (11213.0f/65536.0f);
            z2 = z + (44845.0f/65536.0f);

            float xDistort = x + (( _xDistortModule).GetValue(x0, y0, z0)*_power);
            float yDistort = y + (( _yDistortModule).GetValue(x1, y1, z1)*_power);
            float zDistort = z + (( _zDistortModule).GetValue(x2, y2, z2)*_power);

            // Retrieve the output value at the offsetted input value instead of the
            // original input value.
            return (_sourceModule).GetValue(xDistort, yDistort, zDistort);
        }

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
            // Get the values from the three Perlin noise modules and
            // add each value to each coordinate of the input value.  There are also
            // some offsets added to the coordinates of the input values.  This prevents
            // the distortion modules from returning zero if the (x, y, z) coordinates,
            // when multiplied by the frequency, are near an integer boundary.  This is
            // due to a property of gradient coherent noise, which returns zero at
            // integer boundaries.
            float x0, y0;
            float x1, y1;
            float x2, y2;

            x0 = x + (12414.0f/65536.0f);
            y0 = y + (65124.0f/65536.0f);

            x1 = x + (26519.0f/65536.0f);
            y1 = y + (18128.0f/65536.0f);

            float xDistort = x + (( _xDistortModule).GetValue(x0, y0)*_power);
            float yDistort = y + (( _yDistortModule).GetValue(x1, y1)*_power);

            // Retrieve the output value at the offsetted input value instead of the
            // original input value.
            return (_sourceModule).GetValue(xDistort, yDistort);
        }        

        #endregion
    }
}