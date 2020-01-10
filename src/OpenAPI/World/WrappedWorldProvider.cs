using System.Linq;
using System.Numerics;
using MiNET.Utils;
using MiNET.Worlds;
using OpenAPI.Events.Level;

namespace OpenAPI.World
{
    public class WrappedWorldProvider : IWorldProvider
    {
        protected OpenAPI Api { get; }
        private IWorldProvider WorldProvider { get; }
        public OpenLevel Level { get; set; }
        public WrappedWorldProvider(OpenAPI api, IWorldProvider worldProvider)
        {
            Api = api;
            WorldProvider = worldProvider;
        }
        
        public void Initialize()
        {
            WorldProvider.Initialize();
        }

        public virtual ChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates, bool cacheOnly = false)
        {
            var chunk = WorldProvider.GenerateChunkColumn(chunkCoordinates, cacheOnly);
            
            Api.EventDispatcher.DispatchEvent(new ChunkGeneratedEvent(chunkCoordinates, chunk, Level));
            
            return chunk;
        }

        public Vector3 GetSpawnPoint()
        {
            return WorldProvider.GetSpawnPoint();
        }

        public string GetName()
        {
            return WorldProvider.GetName();
        }

        public long GetTime()
        {
            return WorldProvider.GetTime();
        }

        public long GetDayTime()
        {
            return WorldProvider.GetDayTime();
        }

        public int SaveChunks()
        {
            return WorldProvider.SaveChunks();
        }

        public bool HaveNether()
        {
            return WorldProvider.HaveNether();
        }

        public bool HaveTheEnd()
        {
            return WorldProvider.HaveTheEnd();
        }

        public bool IsCaching => WorldProvider.IsCaching;
    }
    
    public class WrappedCachedWorldProvider : WrappedWorldProvider, ICachingWorldProvider
    {
        private ICachingWorldProvider CachingWorldProvider { get; }
        public WrappedCachedWorldProvider(OpenAPI api, IWorldProvider worldProvider) : base(api, worldProvider)
        {
            if (worldProvider is ICachingWorldProvider)
            {
                CachingWorldProvider = (ICachingWorldProvider) worldProvider;
            }
        }

        public override ChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates, bool cacheOnly = false)
        {
            var before = GetCachedChunks();
            
            var chunk = base.GenerateChunkColumn(chunkCoordinates, cacheOnly);

            if (before != null && !before.Any(x => x.x == chunk.x && x.z == chunk.z))
            {
                Api.EventDispatcher.DispatchEvent(new ChunkLoadEvent(chunk, Level));
            }
            
            return chunk;
        }

        public ChunkColumn[] GetCachedChunks()
        {
            return CachingWorldProvider.GetCachedChunks();
        }

        public void ClearCachedChunks()
        {
            CachingWorldProvider.ClearCachedChunks();
        }

        public int UnloadChunks(MiNET.Player[] players, ChunkCoordinates spawn, double maxViewDistance)
        {
            ChunkColumn[] before = GetCachedChunks();
            
            int unloaded = CachingWorldProvider.UnloadChunks(players, spawn, maxViewDistance);

            if (unloaded > 0 && before != null)
            {
                ChunkColumn[] after = GetCachedChunks();
                if (after == null)
                    return unloaded;
                
                var unloadedChunks = after.Where(chunk => !before.Any(x => x.x == chunk.x && x.z == chunk.z));

                foreach (var uc in unloadedChunks)
                {
                    Api.EventDispatcher.DispatchEvent(new ChunkUnloadEvent(uc, Level));
                }
            }


            return unloaded;
        }
    }
}