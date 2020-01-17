using System;
using LibNoise;
using OpenAPI.WorldGenerator.Utils.Noise.Attributes;

namespace OpenAPI.WorldGenerator.Utils.Noise.Modules
{
  public class FilterNoise
  {
    protected float _frequency = 1f;
    protected float _gain = 2f;
    protected float _lacunarity = 2f;
    protected float _octaveCount = 6f;
    protected float _offset = 1f;
    protected float _spectralExponent = 0.9f;
    protected float[] _spectralWeights = new float[30];
    public const float DEFAULT_FREQUENCY = 1f;
    public const float DEFAULT_LACUNARITY = 2f;
    public const float DEFAULT_OCTAVE_COUNT = 6f;
    public const int MAX_OCTAVE = 30;
    public const float DEFAULT_OFFSET = 1f;
    public const float DEFAULT_GAIN = 2f;
    public const float DEFAULT_SPECTRAL_EXPONENT = 0.9f;
    protected INoiseModule _source;

    [Modifier]
    public float Frequency
    {
      get { return this._frequency; }
      set { this._frequency = value; }
    }

    [Modifier]
    public float Lacunarity
    {
      get { return this._lacunarity; }
      set
      {
        this._lacunarity = value;
        this.ComputeSpectralWeights();
      }
    }

    [Modifier]
    public float OctaveCount
    {
      get { return this._octaveCount; }
      set { this._octaveCount = Libnoise.Clamp(value, 1f, 30f); }
    }

    [Modifier]
    public float Offset
    {
      get { return this._offset; }
      set { this._offset = value; }
    }

    [Modifier]
    public float Gain
    {
      get { return this._gain; }
      set { this._gain = value; }
    }

    [Modifier]
    public float SpectralExponent
    {
      get { return this._spectralExponent; }
      set
      {
        this._spectralExponent = value;
        this.ComputeSpectralWeights();
      }
    }

    public INoiseModule Primitive
    {
      get { return this._source; }
      set { this._source = value; }
    }

    protected FilterNoise()
      : this(1f, 2f, 0.9f, 6f)
    {
    }

    protected FilterNoise(float frequency, float lacunarity, float exponent, float octaveCount)
    {
      this._frequency = frequency;
      this._lacunarity = lacunarity;
      this._spectralExponent = exponent;
      this._octaveCount = octaveCount;
      this.ComputeSpectralWeights();
    }

    protected void ComputeSpectralWeights()
    {
      for (int index = 0; index < 30; ++index)
        this._spectralWeights[index] = MathF.Pow(this._lacunarity, -index * this._spectralExponent);
    }
  }
}