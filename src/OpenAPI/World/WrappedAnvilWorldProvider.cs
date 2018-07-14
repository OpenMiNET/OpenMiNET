using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using fNbt;
using log4net;
using MiNET.Utils;
using MiNET.Worlds;
using OpenAPI.Events.Level;

namespace OpenAPI.World
{
	public sealed class WrappedAnvilWorldProvider : IWorldProvider, ICachingWorldProvider, System.ICloneable
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(WrappedAnvilWorldProvider));
		
		public AnvilWorldProvider AnvilProvider { get; internal set; }

		private OpenAPI Api { get; }
		public WrappedAnvilWorldProvider(OpenAPI api)
		{
			Api = api;
			AnvilProvider = new AnvilWorldProvider();
		}

		public WrappedAnvilWorldProvider(OpenAPI api, string basePath)
		{
			Api = api;
			AnvilProvider = new AnvilWorldProvider(basePath);
		}

		private bool UseCached { get; } = true;
		public WrappedAnvilWorldProvider(OpenAPI api, string basePath, bool cached, ChunkColumn[] chunks)
		{
			UseCached = cached;
			Api = api;

			AnvilProvider = new AnvilWorldProvider(basePath);
			foreach(var pair in chunks)
			{//pair.
				var chunk = (ChunkColumn)pair.Clone();
			 /*
			  * new ChunkColumn()
			 {
				 x = pair.x,
				 z = pair.z,
				 //chunks = pair.chunks.Select(x => (Chunk)x.Clone()).ToArray(),
				 biomeId = (byte[]) pair.biomeId.Clone(),
				 height = (short[]) pair.height.Clone(),
				 isAllAir = pair.isAllAir,
				 IsLoaded = pair.IsLoaded,
				 isGenerated = pair.isGenerated,
				 NeedSave = pair.NeedSave,
				 BlockEntities = CloneDictionaryCloningValues(pair.BlockEntities),
				 isDirty = pair.isDirty,
				 isNew = pair.isNew
			 }
			  */
				if (pair == null || !AnvilProvider._chunkCache.TryAdd(new ChunkCoordinates(pair.x, pair.z), chunk))
				{
					Log.Warn("Could not load cached chunk to worldprovider!");
				}
			}
		}
		private  static IDictionary<TKey, TValue> CloneDictionaryCloningValues<TKey, TValue>
			(IDictionary<TKey, TValue> original) where TValue : ICloneable
		{
			IDictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(original.Count);
			foreach (KeyValuePair<TKey, TValue> entry in original)
			{
				ret.Add(entry.Key, (TValue)entry.Value.Clone());
			}
			return ret;
		}

		public IWorldGenerator MissingChunkProvider
		{
			set => AnvilProvider.MissingChunkProvider = value;
			get => AnvilProvider.MissingChunkProvider;
		}

		public Dimension Dimension
		{
			get => AnvilProvider.Dimension;
			set => AnvilProvider.Dimension = value;
		}

		public bool IsCaching => UseCached && AnvilProvider.IsCaching;

		public bool ReadSkyLight {
			get => AnvilProvider.ReadSkyLight;
			set => AnvilProvider.ReadSkyLight = value;
		}

		public bool ReadBlockLight {
			get => AnvilProvider.ReadBlockLight;
			set => AnvilProvider.ReadBlockLight = value;
		}

		public bool Locked {
			get => AnvilProvider.Locked;
			set => AnvilProvider.Locked = value;
		}

		public void Initialize()
		{
			AnvilProvider.Initialize();
		}

		public ChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates, bool cacheOnly)
		{
			var chunk = AnvilProvider.GenerateChunkColumn(chunkCoordinates, cacheOnly);
			if (chunk != null && chunk.IsLoaded)
			{
				ChunkLoadEvent loadEvent = new ChunkLoadEvent(chunk);
				Api.EventDispatcher.DispatchEvent(loadEvent);
			}
			return chunk;
		}

		public System.Numerics.Vector3 GetSpawnPoint()
		{
			return AnvilProvider.GetSpawnPoint();
		}

		public string GetName()
		{
			return AnvilProvider.GetName();
		}

		public long GetTime()
		{
			return AnvilProvider.GetTime();
		}

		public int SaveChunks()
		{
			int count = 0;
			try
			{
				lock (AnvilProvider._chunkCache)
				{
					AnvilProvider.SaveLevelInfo(AnvilProvider.LevelInfo);

					Dictionary<Tuple<int, int>, List<ChunkColumn>> regions = new Dictionary<Tuple<int, int>, List<ChunkColumn>>();
					foreach (var chunkColumn in AnvilProvider._chunkCache.OrderBy(pair => pair.Key.X >> 5).ThenBy(pair => pair.Key.Z >> 5))
					{
						var regionKey = new Tuple<int, int>(chunkColumn.Key.X >> 5, chunkColumn.Key.Z >> 5);
						if (!regions.ContainsKey(regionKey))
						{
							regions.Add(regionKey, new List<ChunkColumn>());
						}

						regions[regionKey].Add(chunkColumn.Value);
					}

					List<Task> tasks = new List<Task>();
					foreach (var region in regions.OrderBy(pair => pair.Key.Item1).ThenBy(pair => pair.Key.Item2))
					{
						Task task = new Task(delegate
						{
							List<ChunkColumn> chunks = region.Value;
							foreach (var chunkColumn in chunks)
							{
								if (chunkColumn != null && chunkColumn.NeedSave)
								{
									ChunkSaveEvent saveEvent = new ChunkSaveEvent(chunkColumn);
									Api.EventDispatcher.DispatchEvent(saveEvent);
									if (saveEvent.IsCancelled)
									{
										continue;
									}

									AnvilWorldProvider.SaveChunk(chunkColumn, AnvilProvider.BasePath);
									count++;
								}
							}
						});
						task.Start();
						tasks.Add(task);
					}

					Task.WaitAll(tasks.ToArray());

					//foreach (var chunkColumn in _chunkCache.OrderBy(pair => pair.Key.X >> 5).ThenBy(pair => pair.Key.Z >> 5))
					//{
					//	if (chunkColumn.Value != null && chunkColumn.Value.NeedSave)
					//	{
					//		SaveChunk(chunkColumn.Value, BasePath);
					//		count++;
					//	}
					//}
				}
			}
			catch (Exception e)
			{
				Log.Error("saving chunks", e);
			}

			//LevelSaveEvent levelSaveEvent = new LevelSaveEvent();


			return count;
		}

		public bool HaveNether()
		{
			return AnvilProvider.HaveNether();
		}

		public bool HaveTheEnd()
		{
			return AnvilProvider.HaveTheEnd();
		}

		public ChunkColumn[] GetCachedChunks()
		{
			return AnvilProvider.GetCachedChunks();
		}

		public void ClearCachedChunks()
		{
			AnvilProvider.ClearCachedChunks();
		}

		public int UnloadChunks(MiNET.Player[] players, ChunkCoordinates spawn, double maxViewDistance)
		{
			return 0;
		}

		public object Clone()
		{
			return new WrappedAnvilWorldProvider(Api)
			{
				AnvilProvider = (AnvilWorldProvider) AnvilProvider.Clone()
			};
		}

		public long GetDayTime()
		{
			return AnvilProvider.GetDayTime();
		}
	}
}
