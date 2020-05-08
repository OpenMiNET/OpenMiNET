using System;
using System.Diagnostics;
using System.Reflection.Metadata;
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
        public float startheight;//0-2
        public float stopheight;
        public int heightvariation;
        public bool waterbiome = false;


        public BiomeQualifications(float startrain, float stoprain, float starttemp, float stoptemp, float startheight, float stopheight, int heightvariation,bool waterbiome = false)
        {
            this.startrain = startrain;
            this.stoprain = stoprain;
            this.starttemp = starttemp;
            this.stoptemp = stoptemp;
            this.startheight = startheight;
            this.stopheight = stopheight;
            this.waterbiome = waterbiome;
            this.heightvariation = heightvariation;
        }


        public bool check( float[] rth)
        {
            float rain=rth[0];
            float temp=rth[1];
            float height=rth[2];
            return (startrain <= rain && stoprain >= rain && starttemp <= temp && stoptemp >= temp && startheight <= height && stopheight >= height );
        }
        public bool check( float rain,float temp,float height)
        {
            return (startrain <= rain && stoprain >= rain && starttemp <= temp && stoptemp >= temp && startheight <= height && stopheight >= height );
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
        public bool check( float[] rth)
        {
            return BiomeQualifications.check(rth);
        }
        public void prePopulate(ChunkColumn chunk, float[] rth)
        {
            // var t = new Stopwatch();
            // t.Start();
            PopulateChunk(chunk,rth);
            // t.Stop();
            // Console.WriteLine($"CHUNK POPULATION OF {chunk.X} {chunk.Z} TOOK {t.Elapsed}");
        }

        /// <summary>
        /// Populate Chunk from Biome
        /// </summary>
        /// <param name="c"></param>
        public abstract void PopulateChunk(ChunkColumn c, float[] rth);


     
        
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