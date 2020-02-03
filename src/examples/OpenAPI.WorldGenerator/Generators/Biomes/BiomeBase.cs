using MiNET.Utils;
using MiNET.Worlds;
using OpenAPI.WorldGenerator.Generators.Biomes.Config;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Beach;
using OpenAPI.WorldGenerator.Generators.Surfaces;
using OpenAPI.WorldGenerator.Generators.Terrain;
using OpenAPI.WorldGenerator.Utils.Noise;
using OpenAPI.WorldGenerator.Utils.Noise.Api;
using OpenAPI.WorldGenerator.Utils.Noise.Cellular;

namespace OpenAPI.WorldGenerator.Generators.Biomes
{
    public class BiomeBase : MiNET.Worlds.Biome
    {
        public BiomeConfig Config { get; } = new BiomeConfig();
        
        public float MinHeight = 0.1f;
        public float MaxHeight = 0.3f;

        public byte SurfaceBlock = 2;
        public byte SurfaceMetadata = 0;

        public byte SoilBlock = 3;
        public byte SoilMetadata = 0;

        private BiomeType ForcedType { get; set; } = BiomeType.Unknown;
        public BiomeType Type
        {
            get
            {
                if (ForcedType == BiomeType.Unknown)
                    return DetermineType();

                return ForcedType;
            }
            set { ForcedType = value; }
        }
        
        public BiomeBase()
        {
            Type = BiomeType.Unknown;
        }
        
        public TerrainBase Terrain { get; set; } = null;
        public SurfaceBase Surface { get; set; } = null;
        
        public float RNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river)
        {
            // we now have both lakes and rivers lowering land
            if (!this.Config.AllowRivers)
            {
                float borderForRiver = border * 2;
                if (borderForRiver > 1f)
                {
                    borderForRiver = 1;
                }

                river = 1f - (1f - borderForRiver) * (1f - river);
                return Terrain.GenerateNoise(generator, x, y, border, river);
            }

            float lakeStrength = LakePressure(generator, x, y, border, generator.LakeFrequency,
                OverworldGeneratorV2.LakeBendSizeLarge, OverworldGeneratorV2.LakeBendSizeMedium, OverworldGeneratorV2.LakeBendSizeSmall);
            float lakeFlattening = LakeFlattening(lakeStrength, generator.LakeShoreLevel,
                generator.LakeDepressionLevel);

            // combine rivers and lakes
            if ((river < 1) && (lakeFlattening < 1))
            {
                river = (1f - river) / river + (1f - lakeFlattening) / lakeFlattening;
                river = (1f / (river + 1f));
            }
            else if (lakeFlattening < river)
            {
                river = lakeFlattening;
            }

            // smooth the edges on the top
            river = 1f - river;
            river = river * (river / (river + 0.05f) * (1.05f));
            river = 1f - river;

            // make the water areas flat for water features
            float riverFlattening = river * (1f + OverworldGeneratorV2.RiverFlatteningAddend) - OverworldGeneratorV2.RiverFlatteningAddend;
            if (riverFlattening < 0)
            {
                riverFlattening = 0;
            }

            // flatten terrain to set up for the water features
            float terrainNoise = Terrain.GenerateNoise(generator, x, y, border, riverFlattening);
            // place water features
            return this.ErodedNoise(generator, x, y, river, border, terrainNoise);
        }

        public float ErodedNoise(OverworldGeneratorV2 generator, int x, int y, float river, float border, float biomeHeight)
        {
            float r;
            // river of actualRiverProportions now maps to 1;
            float riverFlattening = 1f - river;
            riverFlattening = riverFlattening - (1 - OverworldGeneratorV2.ActualRiverProportion);
            // return biomeHeight if no river effect
            if (riverFlattening < 0)
            {
                return biomeHeight;
            }

            // what was 1 set back to 1;
            riverFlattening /= OverworldGeneratorV2.ActualRiverProportion;

            // back to usual meanings: 1 = no river 0 = river
            r = 1f - riverFlattening;

            if ((r < 1f && biomeHeight > 55f))
            {
                float irregularity = generator.SimplexInstance(0).GetValue(x / 12f, y / 12f) * 2f +
                                     generator.SimplexInstance(0).GetValue(x / 8f, y / 8f);
                // less on the bottom and more on the sides
                irregularity = irregularity * (1 + r);
                return (biomeHeight * (r)) + ((55f + irregularity) * 1.0f) * (1f - r);
            }
            else
            {
                return biomeHeight;
            }

            return biomeHeight;
        }

        public float LakePressure(OverworldGeneratorV2 generator, int x, int y, float border, float lakeInterval,
            float largeBendSize, float mediumBendSize, float smallBendSize)
        {
            
            if (!this.Config.AllowScenicLakes)
            {
                return 1f;
            }

            double pX = x;
            double pY = y;
            ISimplexData2D jitterData = SimplexData2D.NewDisk();

            generator.SimplexInstance(1).GetValue(x / 240.0d, y / 240.0d, jitterData);
            pX += jitterData.GetDeltaX() * largeBendSize;
            pY += jitterData.GetDeltaY() * largeBendSize;

            generator.SimplexInstance(0).GetValue(x / 80.0d, y / 80.0d, jitterData);
            pX += jitterData.GetDeltaX() * mediumBendSize;
            pY += jitterData.GetDeltaY() * mediumBendSize;

            generator.SimplexInstance(4).GetValue(x / 30.0d, y / 30.0d, jitterData);
            pX += jitterData.GetDeltaX() * smallBendSize;
            pY += jitterData.GetDeltaY() * smallBendSize;

            VoronoiResult lakeResults = generator.CellularInstance(0).Eval2D(pX / lakeInterval, pY / lakeInterval);
            return (float) (1.0d - lakeResults.InteriorValue);
        }

        public float LakeFlattening(float pressure, float shoreLevel, float topLevel)
        {
            // adjusts the lake pressure to the river numbers. The lake shoreLevel is mapped
            // to become equivalent to actualRiverProportion
            if (pressure > topLevel)
            {
                return 1;
            }

            if (pressure < shoreLevel)
            {
                return (pressure / shoreLevel) * OverworldGeneratorV2.ActualRiverProportion;
            }

            // proportion between top and shore becomes proportion between 1 and actual river
            float proportion = (pressure - shoreLevel) / (topLevel - shoreLevel);
            return OverworldGeneratorV2.ActualRiverProportion + proportion * (1f - OverworldGeneratorV2.ActualRiverProportion);
            //return (float)Math.pow((pressure-shoreLevel)/(topLevel-shoreLevel),1.0);
          return 1;
        }

        public void Replace(ChunkColumn primer, BlockCoordinates blockPos, int x, int y, int depth, OverworldGeneratorV2 generator,
            float[] noise, float river, BiomeBase[] biomes)
        {
            Replace(primer, blockPos.X, blockPos.Z, x, y, depth, generator, noise, river, biomes);
        }

        public void Replace(ChunkColumn primer, int i, int j, int x, int y, int depth, OverworldGeneratorV2 generator,
            float[] noise, float river, BiomeBase[] biomes)
        {
          /*  if (RTG.surfacesDisabled() || this.getConfig().DISABLE_RTG_SURFACES.get())
            {
                return;
            }
*/
          if (this.Surface == null)
              return;
          
            float riverRegion = !this.Config.AllowRivers ? 0f : river;
            this.Surface.PaintTerrain(primer, i, j, x, y, depth, generator, noise, riverRegion, biomes);
        }

        protected void ReplaceWithRiver(ChunkColumn primer, int i, int j, int x, int y, int depth, OverworldGeneratorV2 generator,
            float[] noise, float river, BiomeBase[] biomes)
        {
        //    if (RTG.surfacesDisabled() || this.getConfig().DISABLE_RTG_SURFACES.get())
        //    {
        //        return;
        //    }

            float riverRegion = !this.Config.AllowRivers ? 0f : river;
            this.Surface.PaintTerrain(primer, i, j, x, y, depth, generator, noise, riverRegion, biomes);
           /* if (RTGConfig.lushRiverbanksInDesert())
            {
                this.surfaceRiver.paintTerrain(primer, i, j, x, y, depth, generator, noise, riverRegion, biomes);
            }*/
        }

       private BiomeType DetermineType()
       {
           BiomeType flags = BiomeType.Land;
           
           if (Temperature <= 0.05f)
           {
               flags |= BiomeType.Cold;

               if (Temperature < 0f)
               {
                   flags |= BiomeType.Snowy;
               }
           }
           else if (Temperature > 0.75f)
           {
               flags |= BiomeType.HotHotHot;
           }
           
           if (MinHeight <= -1f)
           {
               flags |= BiomeType.Ocean;
           }
           else
           {
               if (Downfall <= 0f)
               {
                   flags |= BiomeType.Desert;
               }
               else if (Temperature >= 0.8f && Downfall >= 0.8f)
               {
                   flags |= BiomeType.Swamp;
               }
               else if (Downfall > 0.5f)
               {
                   flags |= BiomeType.Forest;

                   if (Temperature <= 0.01f)
                       flags |= BiomeType.Coniferous;
               }
           }

           return flags;
       }

       public int GetRiverBiome()
       {
           if ((Type & BiomeType.Snowy) != 0)
           {
               return new FrozenRiverBiome().Id;
           }
           else
           {
               return new RiverBiome().Id;
           }
       }

       public int GetBeachBiome()
       {
           var beachType = DetermineBeachType();
           if (beachType == BeachType.Cold)
               return new ColdBeachBiome().Id;
           
           if (beachType == BeachType.Normal)
               return new BeachBiome().Id;
           
           return new StoneBeachBiome().Id;
       }
       
       
       protected BeachType DetermineBeachType()
       {
           if (Temperature <= 0.05f || Type.HasFlag(BiomeType.Snowy))
               return BeachType.Cold;

           if (IsTaigaBiome())
               return BeachType.Stone;

           return BeachType.Normal;
       }

       protected bool IsTaigaBiome()
       {
           return Type.HasFlag(BiomeType.Cold) && Type.HasFlag(BiomeType.Coniferous) && Type.HasFlag(BiomeType.Forest);
       }
    }
}