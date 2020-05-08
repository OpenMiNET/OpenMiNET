using System;
using System.Diagnostics;
using MiNET.Utils;
using MiNET.Worlds;

namespace OpenAPI.World
{
    public class BiomeQualifications
    {
        public float startrain;//0 - 1
        public float stoprain;
        public float starttemp;//0 - 2
        public float stoptemp;
        public int heightvariation;
        public bool waterbiome = false;

        public BiomeQualifications(float startrain, float stoprain, float starttemp, float stoptemp, int heightvariation, bool waterbiome = false)
        {
            this.startrain = startrain;
            this.stoprain = stoprain;
            this.starttemp = starttemp;
            this.stoptemp = stoptemp;
            this.heightvariation = heightvariation;
            this.waterbiome = waterbiome;
        }
        public bool check(float temp, float rain)
        {
            return (startrain <= rain && stoprain >= rain && starttemp <= temp && stoptemp >= temp);
        }
    }
    
    public abstract class AdvancedBiome
    {
        public String name;
       
        public int startheight = 80;
        public BiomeQualifications BiomeQualifications;
        public AdvancedBiome(string name, BiomeQualifications bq)
        {
            BiomeQualifications = bq;
            this.name = name;
        }
        public bool check(float temp, float rain)
        {
            return BiomeQualifications.check(temp, rain);
        }
        public void prePopulate(ChunkColumn chunk, float rain, float temp)
        {
            // var t = new Stopwatch();
            // t.Start();
            PopulateChunk(chunk,rain,temp);
            // t.Stop();
            // Console.WriteLine($"CHUNK POPULATION OF {chunk.X} {chunk.Z} TOOK {t.Elapsed}");
        }

        /// <summary>
        /// Populate Chunk from Biome
        /// </summary>
        /// <param name="c"></param>
        public abstract void PopulateChunk(ChunkColumn c, float rain, float temp);


     
        
        public static AdvancedBiome GetBiome(int biomeId)
        {
            // return Biomes.FirstOrDefault(biome => biome.Id == biomeId) ?? new OpenBiome
            // {
            //     Id = biomeId,
            //     Name = "" + biomeId
            // };
            return new MainBiome();
        }
        
        private static readonly OpenSimplexNoise OpenNoise = new OpenSimplexNoise("a-seed".GetHashCode());

        
        public static int GetNoise(int x, int z, float scale, int max)
        {
            return (int) Math.Floor((OpenNoise.Evaluate(x * scale, z * scale) + 1f) * (max / 2f));
        }
    }
}