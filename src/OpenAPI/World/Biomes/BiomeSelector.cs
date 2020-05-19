using System;
using MiNET.Worlds;

namespace OpenAPI.World
{
    public class BiomeSelector
    {
        private readonly FastNoise hills;
        private readonly FastNoise ocean;
        private readonly FastNoise rainfall;
        private readonly FastNoise river;
        private readonly FastNoise temperature;

        public BiomeSelector(int seed)
        {
            temperature = new FastNoise();
            temperature.SetSeed(seed);
            temperature.SetFrequency(.125f);
            temperature.SetFractalLacunarity(.0009f);
            temperature.SetFractalOctaves(2);
            temperature.SetFractalGain(.5f);
            // this.temperature = new SimplexF(random, 2F, 1F / 8F, 1F / 2048f);
            rainfall = new FastNoise();
            rainfall.SetSeed(seed + 1);
            rainfall.SetFrequency(.125f);
            rainfall.SetFractalLacunarity(.0009f);
            rainfall.SetFractalOctaves(2);
            rainfall.SetFractalGain(.5f);

            river = new FastNoise();
            river.SetSeed(seed + 5);
            river.SetFrequency(.05f);
            river.SetFractalLacunarity(.5f);
            river.SetFractalOctaves(2);
            river.SetFractalGain(.0009f);
            river.SetFractalType(FastNoise.FractalType.Billow);
            //INVERT WITH * -1

            // this.rainfall = new SimplexF(random, 2F, 1F / 8F, 1F / 2048f);
            // this.river = new SimplexF(random, 6f, 2 / 4f, 1 / 1024f);
            // this.ocean = new SimplexF(random, 6f, 2 / 4f, 1 / 2048f);
            ocean = new FastNoise();
            ocean.SetSeed(seed + 2);
            ocean.SetFrequency(.05f);
            ocean.SetFractalLacunarity(.5f);
            ocean.SetFractalOctaves(2);
            ocean.SetFractalGain(.0009f);
            // this.hills = new SimplexF(random, 2f, 2 / 4f, 1 / 2048f);
            hills = new FastNoise();
            hills.SetSeed(seed + 3);
            hills.SetFrequency(.05f);
            hills.SetFractalLacunarity(.5f);
            hills.SetFractalOctaves(2);
            hills.SetFractalGain(.0009f);
        }

        public Biome pickBiome(int x, int z)
        {
            /*double noiseOcean = ocean.noise2D(x, z, true);
            double noiseTemp = temperature.noise2D(x, z, true);
            double noiseRain = rainfall.noise2D(x, z, true);
            if (noiseOcean < -0.15) {
                if (noiseOcean < -0.9) {
                    return BiomeUtils.GetBiome(BiomeList.MUSHROOM_ISLAND.biome;
                } else {
                    return BiomeUtils.GetBiome(BiomeList.OCEAN.biome;
                }
            }
            double noiseRiver = Math.abs(river.noise2D(x, z, true));
            if (noiseRiver < 0.04) {
                return BiomeUtils.GetBiome(BiomeList.RIVER.biome;
            }
            return BiomeUtils.GetBiome(BiomeList.OCEAN.biome;*/

            // > using actual biome selectors in 2018
            //x >>= 6;
            //z >>= 6;

            //here's a test for just every biome, for making sure there's no crashes:
            //return Biome.unorderedBiomes.get(Math.abs(((int) x >> 5) ^ 6457109 * ((int) z >> 5) ^ 9800471) % Biome.unorderedBiomes.size());

            //a couple random high primes: 6457109 9800471 7003231

            //here's a test for mesas
            /*boolean doPlateau = ocean.noise2D(x, z, true) < 0f;
            boolean doF = rainfall.noise2D(x, z, true) < -0.5f;
            if (doPlateau)  {
                boolean doM = temperature.noise2D(x, z, true) < 0f;
                if (doM && doF)    {
                    return BiomeUtils.GetBiome(BiomeList.MESA_PLATEAU_F_M.biome;
                } else if (doM) {
                    return BiomeUtils.GetBiome(BiomeList.MESA_PLATEAU_M.biome;
                } else if (doF) {
                    return BiomeUtils.GetBiome(BiomeList.MESA_PLATEAU_F.biome;
                } else {
                    return BiomeUtils.GetBiome(BiomeList.MESA_PLATEAU.biome;
                }
            } else {
                return doF ? BiomeUtils.GetBiome(BiomeList.MESA_BRYCE.biome : BiomeUtils.GetBiome(BiomeList.MESA.biome;
            }*/

            //here's a test for extreme hills + oceans
            /*double noiseOcean = ocean.noise2D(x, z, true);
            if (noiseOcean < -0.15f) {
                return BiomeUtils.GetBiome(BiomeList.OCEAN.biome;
            } else if (noiseOcean < -0.19f) {
                return BiomeUtils.GetBiome(BiomeList.STONE_BEACH.biome;
            } else {
                boolean plus = temperature.noise2D(x, z, true) < 0f;
                boolean m = rainfall.noise2D(x, z, true) < 0f;
                if (plus && m) {
                    return BiomeUtils.GetBiome(BiomeList.EXTREME_HILLS_PLUS_M.biome;
                } else if (m) {
                    return BiomeUtils.GetBiome(BiomeList.EXTREME_HILLS_M.biome;
                } else if (plus) {
                    return BiomeUtils.GetBiome(BiomeList.EXTREME_HILLS_PLUS.biome;
                } else {
                    return BiomeUtils.GetBiome(BiomeList.EXTREME_HILLS.biome;
                }
            }*/

            var noiseOcean = ocean.GetNoise(x, z);
            var noiseRiver = river.GetNoise(x, z) * -1;
            var temperature = this.temperature.GetNoise(x, z);
            var rainfall = this.rainfall.GetNoise(x, z);
            Biome biome;
            if (noiseOcean < -0.15f)
            {
                if (noiseOcean < -0.91f)
                {
                    if (noiseOcean < -0.92f)
                        biome = BiomeUtils.GetBiome(BiomeList.MUSHROOM_ISLAND);
                    else
                        biome = BiomeUtils.GetBiome(BiomeList.MUSHROOM_ISLAND_SHORE);
                }
                else
                {
                    if (rainfall < 0f)
                        biome = BiomeUtils.GetBiome(BiomeList.OCEAN);
                    else
                        biome = BiomeUtils.GetBiome(BiomeList.DEEP_OCEAN);
                }
            }
            else if (Math.Abs(noiseRiver) < 0.04f)
            {
                if (temperature < -0.3f)
                    biome = BiomeUtils.GetBiome(BiomeList.FROZEN_RIVER);
                else
                    biome = BiomeUtils.GetBiome(BiomeList.RIVER);
            }
            else
            {
                var hills = this.hills.GetNoise(x, z);
                if (temperature < -0.379f)
                {
                    //freezing
                    if (noiseOcean < -0.12f)
                    {
                        biome = BiomeUtils.GetBiome(BiomeList.COLD_BEACH);
                    }
                    else if (rainfall < 0f)
                    {
                        if (hills < -0.1f)
                            biome = BiomeUtils.GetBiome(BiomeList.COLD_TAIGA);
                        else if (hills < 0.5f)
                            biome = BiomeUtils.GetBiome(BiomeList.COLD_TAIGA_HILLS);
                        else
                            biome = BiomeUtils.GetBiome(BiomeList.COLD_TAIGA_M);
                    }
                    else
                    {
                        if (hills < 0.7f)
                            biome = BiomeUtils.GetBiome(BiomeList.ICE_PLAINS);
                        else
                            biome = BiomeUtils.GetBiome(BiomeList.ICE_PLAINS_SPIKES);
                    }
                }
                else if (noiseOcean < -0.12f)
                {
                    biome = BiomeUtils.GetBiome(BiomeList.BEACH);
                }
                else if (temperature < 0f)
                {
                    //cold
                    if (hills < 0.2f)
                    {
                        if (rainfall < -0.5f)
                            biome = BiomeUtils.GetBiome(BiomeList.EXTREME_HILLS_M);
                        else if (rainfall > 0.5f)
                            biome = BiomeUtils.GetBiome(BiomeList.EXTREME_HILLS_PLUS_M);
                        else if (rainfall < 0f)
                            biome = BiomeUtils.GetBiome(BiomeList.EXTREME_HILLS);
                        else
                            biome = BiomeUtils.GetBiome(BiomeList.EXTREME_HILLS_PLUS);
                    }
                    else
                    {
                        if (rainfall < -0.6)
                            biome = BiomeUtils.GetBiome(BiomeList.MEGA_TAIGA);
                        else if (rainfall > 0.6)
                            biome = BiomeUtils.GetBiome(BiomeList.MEGA_SPRUCE_TAIGA);
                        else if (rainfall < 0.2f)
                            biome = BiomeUtils.GetBiome(BiomeList.TAIGA);
                        else
                            biome = BiomeUtils.GetBiome(BiomeList.TAIGA_M);
                    }
                }
                else if (temperature < 0.5f)
                {
                    //normal
                    if (temperature < 0.25f)
                    {
                        if (rainfall < 0f)
                        {
                            if (noiseOcean < 0f)
                                biome = BiomeUtils.GetBiome(BiomeList.SUNFLOWER_PLAINS);
                            else
                                biome = BiomeUtils.GetBiome(BiomeList.PLAINS);
                        }
                        else if (rainfall < 0.25f)
                        {
                            if (noiseOcean < 0f)
                                biome = BiomeUtils.GetBiome(BiomeList.FLOWER_FOREST);
                            else
                                biome = BiomeUtils.GetBiome(BiomeList.FOREST);
                        }
                        else
                        {
                            if (noiseOcean < 0f)
                                biome = BiomeUtils.GetBiome(BiomeList.BIRCH_FOREST_M);
                            else
                                biome = BiomeUtils.GetBiome(BiomeList.BIRCH_FOREST);
                        }
                    }
                    else
                    {
                        if (rainfall < -0.2f)
                        {
                            if (noiseOcean < 0f)
                                biome = BiomeUtils.GetBiome(BiomeList.SWAMPLAND_M);
                            else
                                biome = BiomeUtils.GetBiome(BiomeList.SWAMP);
                        }
                        else if (rainfall > 0.1f)
                        {
                            if (noiseOcean < 0.155f)
                                biome = BiomeUtils.GetBiome(BiomeList.JUNGLE_M);
                            else
                                biome = BiomeUtils.GetBiome(BiomeList.JUNGLE);
                        }
                        else
                        {
                            if (noiseOcean < 0f)
                                biome = BiomeUtils.GetBiome(BiomeList.ROOFED_FOREST_M);
                            else
                                biome = BiomeUtils.GetBiome(BiomeList.ROOFED_FOREST);
                        }
                    }
                }
                else
                {
                    //hot
                    if (rainfall < 0f)
                    {
                        if (noiseOcean < 0f)
                            biome = BiomeUtils.GetBiome(BiomeList.DESERT_M);
                        else if (hills < 0f)
                            biome = BiomeUtils.GetBiome(BiomeList.DESERT_HILLS);
                        else
                            biome = BiomeUtils.GetBiome(BiomeList.DESERT);
                    }
                    else if (rainfall > 0.4f)
                    {
                        if (noiseOcean < 0.155f)
                        {
                            if (hills < 0f)
                                biome = BiomeUtils.GetBiome(BiomeList.SAVANNA_PLATEAU_M);
                            else
                                biome = BiomeUtils.GetBiome(BiomeList.SAVANNA_M);
                        }
                        else
                        {
                            if (hills < 0f)
                                biome = BiomeUtils.GetBiome(BiomeList.SAVANNA_PLATEAU);
                            else
                                biome = BiomeUtils.GetBiome(BiomeList.SAVANNA);
                        }
                    }
                    else
                    {
                        if (noiseOcean < 0f)
                        {
                            if (hills < 0f)
                                biome = BiomeUtils.GetBiome(BiomeList.MESA_PLATEAU_F);
                            else
                                biome = BiomeUtils.GetBiome(BiomeList.MESA_PLATEAU_F_M);
                        }
                        else if (hills < 0f)
                        {
                            if (noiseOcean < 0.2f)
                                biome = BiomeUtils.GetBiome(BiomeList.MESA_PLATEAU_M);
                            else
                                biome = BiomeUtils.GetBiome(BiomeList.MESA_PLATEAU);
                        }
                        else
                        {
                            if (noiseOcean < 0.1f)
                                biome = BiomeUtils.GetBiome(BiomeList.MESA_BRYCE);
                            else
                                biome = BiomeUtils.GetBiome(BiomeList.MESA);
                        }
                    }
                }
            }

            return biome;
        }
    }
}