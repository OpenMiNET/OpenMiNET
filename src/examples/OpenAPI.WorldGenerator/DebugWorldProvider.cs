using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using MiNET.Utils;
using MiNET.Worlds;

namespace OpenAPI.WorldGenerator
{
    public class DebugWorldProvider : IWorldProvider, ICachingWorldProvider
    {
        private readonly ConcurrentDictionary<ChunkCoordinates, ChunkColumn> _chunkCache = new ConcurrentDictionary<ChunkCoordinates, ChunkColumn>();
        public bool IsCaching { get; private set; }

        private IWorldGenerator Generator { get; }
        public DebugWorldProvider(IWorldGenerator worldGenerator)
        {
            Generator = worldGenerator;
            IsCaching = true;
        }
        
        public void Initialize()
        {
            //IsCaching = true;
        }

        public ChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates, bool cacheOnly = false)
        {
            ChunkColumn cachedChunk;
            if (_chunkCache.TryGetValue(chunkCoordinates, out cachedChunk)) return cachedChunk;

            if (cacheOnly)
            {
                return null;
            }

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

        public ChunkColumn[] GetCachedChunks()
        {
            return _chunkCache.Values.ToArray();
        }

        public void ClearCachedChunks()
        {
            _chunkCache.Clear();
        }

        public int UnloadChunks(MiNET.Player[] players, ChunkCoordinates spawn, double maxViewDistance)
        {
            int removed = 0;

            lock (_chunkCache)
            {
                List<ChunkCoordinates> coords = new List<ChunkCoordinates> {spawn};

                foreach (var player in players)
                {
                    var chunkCoordinates = new ChunkCoordinates(player.KnownPosition);
                    if (!coords.Contains(chunkCoordinates)) coords.Add(chunkCoordinates);
                }

                Parallel.ForEach(_chunkCache, (chunkColumn) =>
                {
                    bool keep = coords.Exists(c => c.DistanceTo(chunkColumn.Key) < maxViewDistance);
                    if (!keep)
                    {
                        _chunkCache.TryRemove(chunkColumn.Key, out ChunkColumn waste);

                        if (waste != null)
                        {
                            foreach (var chunk in waste)
                            {
                                chunk.PutPool();
                            }
                        }

                        Interlocked.Increment(ref removed);
                    }
                });
            }

            return removed;
        }
    }
}