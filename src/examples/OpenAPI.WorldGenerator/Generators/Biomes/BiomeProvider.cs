using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using OpenAPI.WorldGenerator.Utils;
using OpenAPI.WorldGenerator.Utils.Noise;

namespace OpenAPI.WorldGenerator.Generators.Biomes
{
    public class BiomeProvider
    {
        public INoiseModule RainfallProvider { get; set; }
        public INoiseModule TemperatureProvider { get; set; }
        
        private BiomeBase[] Biomes { get; }
        public BiomeProvider()
        {
            Biomes = BiomeUtils.Biomes.Where(b => b.Terrain != null).ToArray();
        }

        public BiomeBase GetBiome(int x, int z)
        {
            var temperatures = TemperatureProvider.GetValue(x, z);
            var rainfall = RainfallProvider.GetValue(x, z);

            var biomes = Biomes;
            //   height = MathUtils.ConvertRange(-1f, 1f, 0f, 1f, height);

            var temp = MathUtils.ConvertRange(-1.2f, 1.2f, -0.5f, 2.2f,
                temperatures);


            var rain = MathUtils.ConvertRange(-1f, 1f, 0, 1f,
                rainfall);

            float maxt = -1;
            int maxi = 0;
            float sum = 0;
            var weights = new float[biomes.Length];
            for (int i = 0; i < biomes.Length; i++)
            {
                Vector2 d = new Vector2(biomes[i].Temperature - temp, biomes[i].Downfall - rain);
                d.X *= 5;
                d.Y *= 2.5f;

                weights[i] = MathF.Max(0, 1.0f - (d.X * d.X + d.Y * d.Y) * 0.1f);
                if (weights[i] > maxt)
                {
                    maxi = i;
                    maxt = weights[i];
                }

                sum += weights[i];
            }

            for (int i = 0; i < biomes.Length; i++)
            {
                weights[i] /= sum;
            }

            return biomes[maxi];
        }

        public BiomeBase GetBiome(int id)
        {
            return BiomeUtils.GetBiomeById(id);
        }
    }
}