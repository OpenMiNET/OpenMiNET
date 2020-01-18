using LibNoise;
using LibNoise.Primitive;

namespace OpenAPI.WorldGenerator.Utils.Noise
{
	public class OctaveNoise : INoiseModule
	{
		private readonly int _octaves;
		private INoiseModule _provider;

		public float Frequency { get; set; } = 1f;
		public float Amplitude { get; set; } = 1f;
	//	public float Persistence { get; set; } = 1f;

		public OctaveNoise(INoiseModule provider, int octaves)
		{
			_octaves = octaves;

			_provider = provider;
		}


		public float GetValue(float x, float y)
		{
			float result = 0;
			float amp = 1;
			float freq = 1;
			float max = 0;

			//x *= XScale;
			//y *= YScale;

			for(int i = 0; i < _octaves; i++)
			{
				result += _provider.GetValue((x * freq), (y * freq)) * amp;
				max += amp;
				freq *= Frequency;
				amp *= Amplitude;

				/*max += amp;
				amp *= Persistence;
				freq *= Frequency;*/
			}

			//if (normalized)
			{
				result /= max;
			}

			return result;
		}

		public float GetValue(float x, float y, float z)
		{
			float result = 0;
			float amp = 1;
			float freq = 1;
			float max = 0;

		//	x *= XScale;
		//	y *= YScale;
		//	z *= ZScale;

			for(int i = 0; i < _octaves; i++)
			{
				result += _provider.GetValue((x * freq), (y * freq), (z * freq)) * amp;
				max += amp;
				freq *= Frequency;
				amp *= Amplitude;
			}

			//if (normalized)
			{
				result /= max;
			}

			return result;
		}

		/*public double Noise(double x, double y, double z, double w, double frequency, double amplitude)
		{
			return Noise(x, y, z, w, frequency, amplitude, false);
		}

		public double Noise(double x, double y, double z, double w, double frequency, double amplitude, bool normalized)
		{
			double result = 0;
			double amp = 1;
			double freq = 1;
			double max = 0;

			x *= XScale;
			y *= YScale;
			z *= ZScale;
			w *= WScale;

			foreach (var octave in _generators)
			{
				result += octave.GetValue((float)(x * freq), (float)(y * freq), (float)(z * freq), (float)(w * freq)) * amp;
				max += amp;
				freq *= frequency;
				amp *= amplitude;
			}

			if (normalized)
			{
				result /= max;
			}

			return result;
		}

		public double XScale { get; set; }
		public double YScale { get; set; }
		public double ZScale { get; set; }
		public double WScale { get; set; }

		public void SetScale(double scale)
		{
			XScale = scale;
			YScale = scale;
			ZScale = scale;
			WScale = scale;
		}*/
	}

}
