using LibNoise;
using LibNoise.Filter;
using LibNoise.Primitive;
using OpenAPI.Utils;
using OpenAPI.WorldGenerator.Utils.Noise;
using Voronoi = OpenAPI.WorldGenerator.Utils.Noise.Voronoi;

namespace OpenAPI.WorldGenerator.Generators
{
    public class NoiseProvider
    {
        public readonly IModule3D DepthNoise;
        public readonly IModule2D TerrainNoise;
        public readonly IModule2D RainNoise;
        public readonly IModule2D TempNoise;
        public readonly IModule2D BaseHeightNoise;

        public WorldGeneratorPreset GeneratorPreset { get; set; } = new WorldGeneratorPreset();
        private FastRandom FastRandom { get; }
        
        private int Seed { get; }
        
        public NoiseProvider(WorldGeneratorPreset generatorPreset, int seed)
        {
            GeneratorPreset = generatorPreset;
            
            Seed = seed;
            FastRandom = new FastRandom(seed);

            
            var mainLimitNoise = new SimplexPerlin(seed + FastRandom.Next(), NoiseQuality.Fast);
            
            var mainLimitFractal = new LibNoise.Filter.MultiFractal()
            {
                Primitive3D = mainLimitNoise,
                Primitive2D = mainLimitNoise,
                Frequency = MainNoiseFrequency,
                OctaveCount = 4,
                Lacunarity = MainNoiseLacunarity,
                Gain = MainNoiseGain,
                SpectralExponent = MainNoiseSpectralExponent
            };
         
            TerrainNoise = new ScaleableNoise()
            {
                XScale = 1f / GeneratorPreset.CoordinateScale,
                YScale = 1f / GeneratorPreset.HeightScale,
                ZScale = 1f / GeneratorPreset.CoordinateScale,
                Primitive3D = mainLimitFractal,
                Primitive2D = mainLimitFractal
            }; //turbulence;
            
            
            var baseHeightNoise = new SimplexPerlin(seed + FastRandom.Next(), NoiseQuality.Fast);

            var fractal = new Voronoi()
            {
                Primitive2D = new ScaleableNoise()
                {
                    Primitive2D = baseHeightNoise,
                    XScale = 1f / GeneratorPreset.CoordinateScale,
                    ZScale = 1f / GeneratorPreset.CoordinateScale
                },
                OctaveCount = 1,
                Frequency = 1.295f,
                SpectralExponent = 0.25f
                //Distance = true
             //   Distance = false
            };

            BaseHeightNoise = new ScaleableNoise()
            {
                Primitive2D = fractal,
                XScale = 1f / 8f,
                ZScale = 1f / 8f
            };
            
            var depthNoise = new SimplexPerlin(seed + FastRandom.Next(), NoiseQuality.Fast);
            var depthNoiseFractal = new RidgedMultiFractal()
            {
                Primitive2D = depthNoise,
                Primitive3D = depthNoise,
                Frequency = DepthFrequency,
                Lacunarity = DepthLacunarity,
                Gain = DepthNoiseGain,
                OctaveCount = 2,
                SpectralExponent = (float) GeneratorPreset.DepthNoiseScaleExponent
            };

            DepthNoise = new ScaleableNoise
            {
                Primitive2D = depthNoiseFractal,
                Primitive3D = depthNoiseFractal,
                XScale = 1f / GeneratorPreset.MainNoiseScaleX,
                YScale = 1f / GeneratorPreset.MainNoiseScaleY,
                ZScale = 1f / GeneratorPreset.MainNoiseScaleZ
            };
            
            var rainSimplex = new SimplexPerlin(seed+ FastRandom.Next(), NoiseQuality.Fast);
            var rainVoronoi = new Utils.Noise.Voronoi
            {
                Primitive3D = rainSimplex,
                Primitive2D = rainSimplex,
                Distance = false,
                Frequency = RainFallFrequency,
                OctaveCount = 2
            };

            var biomeScaling = (32.3345885f) * GeneratorPreset.BiomeSize;

            var rainNoise = new ScaleableNoise()
            {
                Primitive2D = rainVoronoi,
                Primitive3D = rainVoronoi,
                XScale = 1f / biomeScaling,
                YScale = 1f / biomeScaling,
                ZScale = 1f / biomeScaling
            };

           // GeneratorPreset.bi
            
            IModule2D tempSimplex = new SimplexPerlin(seed + FastRandom.Next(), NoiseQuality.Fast);
            var tempVoronoi = new Utils.Noise.Voronoi
            {
                Primitive2D = tempSimplex,
                Distance = false,
                Frequency = TemperatureFrequency,
                OctaveCount = 2,
                SpectralExponent = 0.25f
            };
            
            /*var tempNoise =  new ScaleableNoise()
            {
                Primitive2D = tempVoronoi,
                Primitive3D = tempVoronoi,
                XScale = 1f / biomeScaling,
                YScale = 1f / biomeScaling,
                ZScale = 1f / biomeScaling
            };
*/
            TempNoise = new ScaleableNoise()
            {
                Primitive2D = tempVoronoi,
                //   Primitive3D = tempSimplex,
                XScale = 1f / biomeScaling,
                YScale = 1f / biomeScaling,
                ZScale = 1f / biomeScaling
            };
            RainNoise = rainNoise;
        }
        
        private float MainNoiseFrequency = 0.295f;
        private float MainNoiseLacunarity = 2.127f;
        private float MainNoiseGain = 2f;//0.256f;
        private float MainNoiseSpectralExponent = 0.5f;//1f;//0.52f;//0.9f;//1.4f;
        private float MainNoiseOffset = 0f;// 0.312f;

        private float TemperatureFrequency = 0.283f;
        private float RainFallFrequency = 1.03f;
        
       // private float MaxHeight = 256;
      //  public static float WaterLevel = 64;
        
        private float DepthFrequency = 0.662f;
        private float DepthLacunarity = 2.375f; //6f;
        private float DepthNoiseGain = 2f;//0.256f;
    }
}