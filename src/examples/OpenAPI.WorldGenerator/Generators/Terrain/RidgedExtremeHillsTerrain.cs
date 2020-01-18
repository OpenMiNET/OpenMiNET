using OpenAPI.WorldGenerator.Generators.Effects;

namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class RidgedExtremeHillsTerrain : TerrainBase
    {
        private readonly HeightEffect _heightIncrease;
        private readonly HeightEffect _multiplier;
        private readonly HeightEffect _groundEffect;
        private readonly float _height;
        private readonly float _width;
        private readonly float _ridgeWidth = 300f;
        private readonly float _valleyFloor = -0.2f;

        public RidgedExtremeHillsTerrain(float landHeight, float baseHeight, float hillWidth)
        {
            _height = landHeight;
            this.BaseHeight = baseHeight;
            _width = hillWidth;

            SpikeEverywhereEffect baseHills = new SpikeEverywhereEffect();
            baseHills.Spiked = new RaiseEffect(_height * 2f / 3f);
            baseHills.Wavelength = _width;
            baseHills.MinimumSimplex = -0.2f;
            baseHills.Octave = 3;
            baseHills.Power = 1.9f;

            SpikeEverywhereEffect additionalHeightSpikes = new SpikeEverywhereEffect();
            additionalHeightSpikes.Spiked = new RaiseEffect(_height / 3f);
            additionalHeightSpikes.Wavelength = _width / 3f;
            additionalHeightSpikes.MinimumSimplex = -0.2f;
            additionalHeightSpikes.Octave = 4;
            additionalHeightSpikes.Power = 1.9f;

            JitterEffect additionalHeight = new JitterEffect();
            additionalHeight.Amplitude = additionalHeightSpikes.Wavelength / 3;
            additionalHeight.Wavelength = additionalHeightSpikes.Wavelength / 2;
            additionalHeight.Jittered = additionalHeightSpikes;

            SpikeEverywhereEffect rougheningSpikes = new SpikeEverywhereEffect();
            rougheningSpikes.Spiked = new RaiseEffect(_height / 8f);
            rougheningSpikes.Wavelength = _width / 10f;
            rougheningSpikes.MinimumSimplex = -0.2f;
            rougheningSpikes.Octave = 5;
            rougheningSpikes.Power = 1.9f;

            JitterEffect roughening = new JitterEffect();
            roughening.Amplitude = rougheningSpikes.Wavelength / 3;
            roughening.Wavelength = rougheningSpikes.Wavelength / 2;
            roughening.Jittered = rougheningSpikes;

            JitterEffect hillJitter = new JitterEffect();
            hillJitter.Amplitude = 15f;
            hillJitter.Wavelength = 50f;
            hillJitter.Jittered = baseHills.Plus(additionalHeight).Plus(roughening);
            _heightIncrease = hillJitter;


            VoronoiBorderEffect ridging = new VoronoiBorderEffect();
            ridging.PointWavelength = _ridgeWidth;
            ridging.Floor = _valleyFloor;
            ridging.MinimumDivisor = .2f;

            JitterEffect ridgeJitter = new JitterEffect();
            ridgeJitter.Amplitude = 15f;
            ridgeJitter.Wavelength = 50f;
            ridgeJitter.Jittered = ridging;

            JitterEffect ridgeJitterrette = new JitterEffect();
            ridgeJitterrette.Amplitude = 5f;
            ridgeJitterrette.Wavelength = 20f;
            ridgeJitterrette.Jittered = ridgeJitter;
            _multiplier = ridgeJitterrette;

            _groundEffect = new GroundEffect(6);
        }

        public override float GenerateNoise(OverworldGeneratorV2 rtgWorld, int x, int y, float border, float river)
        {
            // ground effect is increased by the multiplier
            float groundEffectLevel = _groundEffect.Added(rtgWorld, (float) x, (float) y);
            float ridging = _multiplier.Added(rtgWorld, (float) x, (float) y);
            ridging = TerrainBase.BayesianAdjustment(ridging, 2);
            float result =
                BaseHeight + ridging * (groundEffectLevel + _heightIncrease.Added(rtgWorld, (float) x, (float) y))
                           + groundEffectLevel;
            return TerrainBase.MountainCap(result);
        }
    }
}