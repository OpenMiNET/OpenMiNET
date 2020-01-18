using System;
using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Effects
{
    /**
     * This creates a spiky multiplier going from 0 to 1
     *
     * @author Zeno410
     */
    public class SpikeEverywhereEffect : HeightEffect
    {

        // not going to bother to set up a creator shell to make sure everything is set
        // set defaults to absurd values to crash if they're not set
        // a trio of parameters frequently used together

        public float Wavelength = 0;

        public float MinimumSimplex = int.MaxValue; // normal range is -1 to 1;

        //usually numbers above 0 are often preferred to avoid dead basins
        public int Octave;
        public float Power = 1.6f; // usually a range of 1 to 2
        public HeightEffect Spiked;

        public override float Added(OverworldGeneratorV2 rtgWorld, float x, float y)
        {

            float noise = rtgWorld.SimplexInstance(Octave).GetValue(x / Wavelength, y / Wavelength);
            noise = Math.Abs(noise);
            noise = TerrainBase.BlendedHillHeight(noise, MinimumSimplex);
            noise = TerrainBase.UnsignedPower(noise, Power);
            return noise * Spiked.Added(rtgWorld, x, y);
        }
    }
}