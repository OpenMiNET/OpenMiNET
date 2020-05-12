using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Threading.Tasks;
using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Worlds;
using Newtonsoft.Json.Serialization;

namespace OpenAPI.World
{
    public class OpenExperimentalWorldProvider : IWorldProvider
    {
        private static readonly Random getrandom = new Random();
        private static readonly object syncLock = new object();

        private static readonly OpenSimplexNoise OpenNoise = new OpenSimplexNoise("a-seed".GetHashCode());

        private readonly ConcurrentDictionary<ChunkCoordinates, ChunkColumn> _chunkCache =
            new ConcurrentDictionary<ChunkCoordinates, ChunkColumn>();

        private float dirtBaseHeight = 3;
        private float dirtNoise = 0.004f;
        private float dirtNoiseHeight = 10;
        private readonly int Seed;

        private float stoneBaseHeight = 50;
        private float stoneBaseNoise = 0.05f;
        private float stoneBaseNoiseHeight = 20;
        private float stoneMinHeight = 20;
        private float stoneMountainFrequency = 0.008f;

        private float stoneMountainHeight = 48;
        private int waterLevel = 25;

        public OpenExperimentalWorldProvider(int seed)
        {
            Seed = seed;
            IsCaching = true;
        }

        public bool IsCaching { get; }
        public OpenLevel Level { get; set; }

        public void Initialize()
        {
        }

        public float[] getChunkRTH(ChunkColumn chunk)
        {
            //CALCULATE RAIN
            var rainnoise = new FastNoise(123123);
            rainnoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
            rainnoise.SetFrequency(.007f); //.015
            rainnoise.SetFractalType(FastNoise.FractalType.FBM);
            rainnoise.SetFractalOctaves(1);
            rainnoise.SetFractalLacunarity(.25f);
            rainnoise.SetFractalGain(1);
            //CALCULATE TEMP
            var tempnoise = new FastNoise(123123 + 1);
            tempnoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
            tempnoise.SetFrequency(.004f); //.015f
            tempnoise.SetFractalType(FastNoise.FractalType.FBM);
            tempnoise.SetFractalOctaves(1);
            tempnoise.SetFractalLacunarity(.25f);
            tempnoise.SetFractalGain(1);
            //CALCULATE HEIGHT
            // var heightnoise = new FastNoise(123123 + 2);
            // heightnoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
            // heightnoise.SetFrequency(.015f);
            // heightnoise.SetFractalType(FastNoise.FractalType.FBM);
            // heightnoise.SetFractalOctaves(1);
            // heightnoise.SetFractalLacunarity(.25f);
            // heightnoise.SetFractalGain(1);
            
            
            
            float rain = rainnoise.GetNoise(chunk.X, chunk.Z) + 1;
            float temp = tempnoise.GetNoise(chunk.X, chunk.Z) + 1;
            float height = GetNoise(chunk.X, chunk.Z, 0.015f,2);;
            return new []{rain, temp, height};
        }

        public ChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates, bool cacheOnly = false)
        {
            return GenerateChunkColumn(chunkCoordinates, true, cacheOnly);
        }
        public ChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates, bool smooth = true,bool cacheOnly = false)
        {
            ChunkColumn cachedChunk;
            if (_chunkCache.TryGetValue(chunkCoordinates, out cachedChunk)) return cachedChunk;

                ChunkColumn chunk;
            if (cacheOnly)return null;
            

            chunk = new ChunkColumn();
            chunk.X = chunkCoordinates.X;
            chunk.Z = chunkCoordinates.Z;
            var rth = getChunkRTH(chunk);


            chunk = PopulateChunk(this,chunk, rth).Result;
           
            if(smooth)chunk = SmoothChunk(this,chunk, rth).Result;

            _chunkCache[chunkCoordinates] = chunk;

            return chunk;
        }

        public Vector3 GetSpawnPoint()
        {
            return new Vector3(50, 10, 50);
        }

        public long GetTime()
        {
            return 0;
        }

        public long GetDayTime()
        {
            return 0;
        }

        public string GetName()
        {
            return "Experimental";
        }

        public int SaveChunks()
        {
            return 0;
        }

        public bool HaveNether()
        {
            return false;
        }

        public bool HaveTheEnd()
        {
            return false;
        }

        public async Task<ChunkColumn> SmoothChunk(OpenExperimentalWorldProvider openExperimentalWorldProvider, ChunkColumn chunk,
            float[] rth)
        {
            
            var b = BiomeManager.GetBiome(chunk);
           var a = await b.preSmooth(openExperimentalWorldProvider,chunk, rth);
            return a;
        }
        public async Task<ChunkColumn> PopulateChunk(OpenExperimentalWorldProvider openExperimentalWorldProvider, ChunkColumn chunk,
            float[] rth)
        {
            var b = BiomeManager.GetBiome(chunk);
            // var b = new MainBiome();
            var a = await b.prePopulate(openExperimentalWorldProvider,chunk, rth);
            return a;
            // b.PopulateChunk(chunk, rain, temp);

// Console.WriteLine($"GENERATORED YO BITCH >> {chunk.X} {chunk.Z}");
        }

        private void GenerateTree(ChunkColumn chunk, int x, int treebase, int z)
        {
            var treeheight = GetRandomNumber(4, 5);

            chunk.SetBlock(x, treebase + treeheight + 2, z, new Leaves()); //Top leave

            chunk.SetBlock(x, treebase + treeheight + 1, z + 1, new Leaves());
            chunk.SetBlock(x, treebase + treeheight + 1, z - 1, new Leaves());
            chunk.SetBlock(x + 1, treebase + treeheight + 1, z, new Leaves());
            chunk.SetBlock(x - 1, treebase + treeheight + 1, z, new Leaves());

            chunk.SetBlock(x, treebase + treeheight, z + 1, new Leaves());
            chunk.SetBlock(x, treebase + treeheight, z - 1, new Leaves());
            chunk.SetBlock(x + 1, treebase + treeheight, z, new Leaves());
            chunk.SetBlock(x - 1, treebase + treeheight, z, new Leaves());

            chunk.SetBlock(x + 1, treebase + treeheight, z + 1, new Leaves());
            chunk.SetBlock(x - 1, treebase + treeheight, z - 1, new Leaves());
            chunk.SetBlock(x + 1, treebase + treeheight, z - 1, new Leaves());
            chunk.SetBlock(x - 1, treebase + treeheight, z + 1, new Leaves());

            for (var i = 0; i <= treeheight; i++) chunk.SetBlock(x, treebase + i, z, new Log());
        }

        private static int GetRandomNumber(int min, int max)
        {
            lock (syncLock)
            {
                // synchronize
                return getrandom.Next(min, max);
            }
        }

        public static float GetNoise(int x, int z, float scale, int max)
        {
            return (float)((OpenNoise.Evaluate(x * scale, z * scale) + 1f) * (max / 2f));
        }
    }
}