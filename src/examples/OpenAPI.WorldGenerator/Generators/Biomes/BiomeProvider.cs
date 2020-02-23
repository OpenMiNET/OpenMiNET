using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using MiNET.Blocks;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Beach;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Desert;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.ExtremeHills;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Forest;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Jungle;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Mesa;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Mushroom;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Ocean;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Swamp;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Taiga;
using OpenAPI.WorldGenerator.Generators.Surfaces;
using OpenAPI.WorldGenerator.Utils;
using OpenAPI.WorldGenerator.Utils.Noise;

namespace OpenAPI.WorldGenerator.Generators.Biomes
{
    public class BiomeProvider
    {
        //public INoiseModule RainfallProvider { get; set; }
        //public INoiseModule TemperatureProvider { get; set; }
        
        public BiomeBase[] Biomes { get; }
        public BiomeProvider()
        {
            //Biomes = BiomeUtils.Biomes.Where(b => b.Terrain != null).ToArray();
            Biomes = new BiomeBase[]
            {
                new BirchForestBiome(),
                new BirchForestHillsBiome(),
                /* new BirchForestHillsMBiome(),
                 new BirchForestMBiome(),
                 new ColdTaigaBiome(),
                 new ColdTaigaHillsBiome(),
                 new ColdTaigaMBiome(),*/
                new DeepOceanBiome(),
                new DesertBiome(),
                new DesertHillsBiome(),
                //    new DesertMBiome(),
                new ExtremeHillsBiome(),
                new ExtremeHillsEdgeBiome(),
                /*    new ExtremeHillsMBiome(),
                    new ExtremeHillsPlusBiome(),
                    new ExtremeHillsPlusMBiome(),
                    new FlowerForestBiome(),*/
                new ForestBiome(),
                new ForestHillsBiome(),
                new FrozenOceanBiome(),
                // new IceMountainsBiome(),
                new IcePlainsBiome(),
                // new IcePlainsSpikesBiome(),
                new JungleBiome(),
                new JungleEdgeBiome(),
                //  new JungleEdgeMBiome(),
                new JungleHillsBiome(),
                /*new JungleMBiome(),
                new MegaSpruceTaigaBiome(),
                new MegaTaigaBiome(),
                new MegaTaigaHillsBiome(),*/
                new MesaBiome(),
                //   new MesaBryceBiome(),
                new MesaPlateauBiome(),
                new MesaPlateauFBiome(),
                //  new MesaPlateauFMBiome(),
                //  new MesaPlateauMBiome(),
                new MushroomIslandBiome(),
                new MushroomIslandShoreBiome(),
                new OceanBiome(),
                new PlainsBiome(),
                //    new RedwoodTaigaHillsBiome(),
                //    new RoofedForestBiome(),
                //   new RoofedForestMBiome(),
                new SavannaBiome(),
                //   new SavannaMBiome(),
                new SavannaPlateauBiome(),
                //   new SavannaPlateauMBiome(),
                //   new SunflowerPlainsBiome(),
                new SwamplandBiome(),
                //   new SwamplandMBiome(),
                new TaigaBiome(),
                new TaigaHillsBiome(),
                //  new TaigaMBiome()
                new RiverBiome(),
                new FrozenRiverBiome(),
                
                new BeachBiome().SetEdgeBiome(true), 
                new ColdBeachBiome().SetEdgeBiome(true), 
                new StoneBeachBiome().SetEdgeBiome(true), 
            };

            for (int i = 0; i < Biomes.Length; i++)
            {
                if (Biomes[i].Terrain != null && Biomes[i].Surface == null)
                {
                    var b = Biomes[i];
                    b.Surface = new SurfaceBase(b.Config, BlockFactory.GetBlockById(b.SurfaceBlock), BlockFactory.GetBlockById(b.SoilBlock));
                    Biomes[i] = b;
                }
            }
        }

        public IEnumerable<BiomeBase> GetBiomes()
        {
            return Biomes.Where(x => x.Terrain != null && x.Surface != null && !x.Config.IsEdgeBiome);
        }

        public BiomeBase GetBiome(float temperatures, float rainfall)
        {
          //  var temperatures = TemperatureProvider.GetValue(x, z);
          //  var rainfall = RainfallProvider.GetValue(x, z);

            var biomes = GetBiomes().ToArray();
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
            return Biomes.FirstOrDefault(x => x.Id == id);//.Where(x => x.Terrain != null && x.Surface != null).FirstOrDefault(x => x.Id == id);
        }
    }
}