using LibNoise;

namespace OpenAPI.WorldGenerator.Utils.Noise.Primitives
{
    /// <summary>
    /// Base class for all noise primitive.
    /// </summary>
    public abstract class PrimitiveModule : IModule
    {
        #region Constants

        /// <summary>
        /// Default noise seed for the noise module.
        /// </summary>
        public const int DefaultSeed = 0;

        /// <summary>
        /// Default noise quality for the noise module.
        /// </summary>
        public const NoiseQuality DefaultQuality = NoiseQuality.Standard;

        #endregion

        #region Fields

        /// <summary>
        /// The quality of the Perlin noise.
        /// </summary>
        private NoiseQuality _quality = DefaultQuality;

        /// <summary>
        /// The seed value used by the Perlin-noise function.
        /// </summary>
        private int _seed = DefaultSeed;

        #endregion

        #region Accessors

        /// <summary>
        /// Gets or sets the seed of the perlin noise.
        /// </summary>
        public virtual int Seed
        {
            get { return _seed; }
            set { _seed = value; }
        }

        /// <summary>
        /// Gets or sets the quality
        /// </summary>
        public virtual NoiseQuality Quality
        {
            get { return _quality; }
            set { _quality = value; }
        }

        #endregion

        #region Ctor/Dtor

        /// <summary>
        /// A 0-args constructor
        /// </summary>
        protected PrimitiveModule()
            : this(DefaultSeed, DefaultQuality)
        {
        }

        /// <summary>
        /// A basic connstrucutor
        /// </summary>
        /// <param name="seed"></param>
        protected PrimitiveModule(int seed)
            : this(seed, DefaultQuality)
        {
        }

        /// <summary>
        /// A basic connstrucutor
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="quality"></param>
        protected PrimitiveModule(int seed, NoiseQuality quality)
        {
            _seed = seed;
            _quality = quality;
        }

        #endregion
    }
}