using System;
using System.Collections.Concurrent;
using System.Numerics;
using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Worlds;

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

        public void Initialize()
        {
        }

        public ChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates, bool cacheOnly = false)
        {
            ChunkColumn cachedChunk;
            if (_chunkCache.TryGetValue(chunkCoordinates, out cachedChunk)) return cachedChunk;

            //CALCULATE RAIN
            var rainnoise = new FastNoise(Seed);
            rainnoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
            rainnoise.SetFrequency(.015f);
            rainnoise.SetFractalType(FastNoise.FractalType.FBM);
            rainnoise.SetFractalOctaves(1);
            rainnoise.SetFractalLacunarity(.25f);
            rainnoise.SetFractalGain(1);
            //CALCULATE TEMP
            var tempnoise = new FastNoise(Seed+1);
            tempnoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
            tempnoise.SetFrequency(.015f);
            tempnoise.SetFractalType(FastNoise.FractalType.FBM);
            tempnoise.SetFractalOctaves(1);
            tempnoise.SetFractalLacunarity(.25f);
            tempnoise.SetFractalGain(1);
            
            var chunk = new ChunkColumn();
            chunk.X = chunkCoordinates.X;
            chunk.Z = chunkCoordinates.Z;

            float rain = rainnoise.GetNoise(chunk.X, chunk.Z)+1;
            float temp = tempnoise.GetNoise(chunk.X, chunk.Z)+1;
          
            
            

            PopulateChunk(chunk,rain,temp);

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

        public void PopulateChunk(ChunkColumn chunk, float rain, float temp)
        {
            var b =BiomeManager.GetBiome(rain,temp);
            // var b = new MainBiome();
            b.prePopulate(chunk, rain, temp);
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

        public static int GetNoise(int x, int z, float scale, int max)
        {
            return (int) Math.Floor((OpenNoise.Evaluate(x * scale, z * scale) + 1f) * (max / 2f));
        }
    }
}