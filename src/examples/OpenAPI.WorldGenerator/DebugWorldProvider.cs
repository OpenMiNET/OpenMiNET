using System.Collections.Concurrent;
using System.Numerics;
using MiNET.Utils;
using MiNET.Worlds;

namespace OpenAPI.WorldGenerator
{
    public class DebugWorldProvider : IWorldProvider
    {
        private readonly ConcurrentDictionary<ChunkCoordinates, ChunkColumn> _chunkCache = new ConcurrentDictionary<ChunkCoordinates, ChunkColumn>();
        public bool IsCaching { get; private set; }

        private IWorldGenerator Generator { get; }
        public DebugWorldProvider(IWorldGenerator worldGenerator)
        {
            Generator = worldGenerator;
        }
        
        public void Initialize()
        {
            IsCaching = true;
        }

        public ChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates, bool cacheOnly = false)
        {
            ChunkColumn cachedChunk;
            if (_chunkCache.TryGetValue(chunkCoordinates, out cachedChunk)) return cachedChunk;

            ChunkColumn chunk = Generator.GenerateChunkColumn(chunkCoordinates);
            
            _chunkCache[chunkCoordinates] = chunk;

            return chunk;
        }

        public Vector3 GetSpawnPoint()
        {
            return new Vector3(0, 100, 0);
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
            return "Cool world";
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
    }
}