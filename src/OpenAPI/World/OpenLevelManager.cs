using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MiNET;
using MiNET.Utils;
using MiNET.Worlds;
using OpenAPI.Entities;
using OpenAPI.Events;
using OpenAPI.Events.Level;
using OpenAPI.Utils;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace OpenAPI.World
{
	public delegate void LevelCreated(OpenLevel level);

	public delegate void LevelDestroyed(OpenLevel level);

	public class OpenLevelManager : LevelManager
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(OpenLevelManager));

		private OpenAPI Api { get; }
		private readonly ConcurrentDictionary<string, OpenLevel> _levels = new ConcurrentDictionary<string, OpenLevel>();
		public int LevelCount => _levels.Count;

		private readonly GameMode _gameMode = Config.GetProperty("GameMode", GameMode.Survival);
		private readonly Difficulty _difficulty = Config.GetProperty("Difficulty", Difficulty.Normal);
		private readonly int _viewDistance = Config.GetProperty("ViewDistance", 11);
		private readonly bool _enableBlockTicking = Config.GetProperty("EnableBlockTicking", false);
		private readonly bool _enableChunkTicking = Config.GetProperty("EnableChunkTicking", false);
		private readonly bool _isWorldTimeStarted = Config.GetProperty("IsWorldTimeStarted", false);
		private readonly bool _readSkyLight = !Config.GetProperty("CalculateLights", false);
		private readonly bool _readBlockLight = !Config.GetProperty("CalculateLights", false);

		private string _defaultLevel = "Overworld";

		//public new LevelCreated OnLevelCreated = null;
		//public LevelDestroyed OnLevelDestroyed = null;
		public OpenLevelManager(OpenAPI api)
		{
			Api = api;
			EntityManager = new OpenEntityManager();
		}

		private string GetLevelId(string levelId)
		{
			int i = 0;
			if (!_levels.IsEmpty)
			{
				var levels = _levels.Keys.Where(x => x.StartsWith(levelId)).ToArray();
				if (levels.Length > 0)
				{
					i = levels.Max(id =>
					{
						var m = Regex.Match(id, @"\d+");

						int r;
						if (m.Success && int.TryParse(m.Value, out r))
						{
							return r;
						}
						return 0;
					});
				}
			}

			return $"{levelId}_{i + 1}";
		}

		public override Level GetLevel(MiNET.Player player, string levelId)
		{
			OpenLevel level;
			if (_levels.TryGetValue(levelId, out level))
			{
				return level;
			}

			string newLevelid;
			if (levelId.Equals(_defaultLevel, StringComparison.InvariantCultureIgnoreCase))
			{
				newLevelid = levelId;
			}
			else
			{
				newLevelid = GetLevelId(levelId);
			}

			var worldProvider = new WrappedAnvilWorldProvider(Api)
			{
				MissingChunkProvider = new AirWorldGenerator(),
				ReadSkyLight = _readSkyLight,
				ReadBlockLight = _readBlockLight
			};

			var openLevel = new OpenLevel(Api/*, Api.EventDispatcher*/, this, newLevelid, worldProvider, EntityManager, _gameMode, _difficulty, _viewDistance)
			{
				EnableBlockTicking = _enableBlockTicking,
				EnableChunkTicking = _enableChunkTicking,
				
			//	IsWorldTimeStarted = _isWorldTimeStarted
			};

			LoadLevel(openLevel);

			return openLevel;
		}

		public OpenLevel GetLevel(string basepath, string levelId, ChunkColumn[] chunks)
		{
			var newLevelId = GetLevelId(levelId);

			var worldProvider = new WrappedAnvilWorldProvider(Api, basepath, false, chunks)
			{
				MissingChunkProvider = new AirWorldGenerator(),
				ReadSkyLight = _readSkyLight,
				ReadBlockLight = _readBlockLight
			};

			var openLevel = new OpenLevel(Api/*, Api.EventDispatcher*/, this, newLevelId, worldProvider, EntityManager, _gameMode, _difficulty, _viewDistance)
			{
				EnableBlockTicking = _enableBlockTicking,
				EnableChunkTicking = _enableChunkTicking,
				//IsWorldTimeStarted = _isWorldTimeStarted
			};

			LoadLevel(openLevel);

			return openLevel;
		}

		public OpenLevel LoadLevel(string levelDirectory)
		{
			return LoadLevel(levelDirectory, levelDirectory.Replace(Path.DirectorySeparatorChar, '_'));
		}

		public OpenLevel LoadLevel(string levelDirectory, string levelId)
		{
			var newLevelId = GetLevelId(levelId);

			var worldProvider = new WrappedAnvilWorldProvider(Api, levelDirectory)
			{
				MissingChunkProvider = new AirWorldGenerator(),
				ReadSkyLight = !Config.GetProperty("CalculateLights", false),
				ReadBlockLight = !Config.GetProperty("CalculateLights", false),
			};

			var openLevel = new OpenLevel(Api/*, Api.EventDispatcher*/, this, newLevelId, worldProvider, EntityManager, _gameMode, _difficulty, _viewDistance)
			{
				EnableBlockTicking = _enableBlockTicking,
				EnableChunkTicking = _enableChunkTicking,
			//	IsWorldTimeStarted = _isWorldTimeStarted
			};

			LoadLevel(openLevel);

			return openLevel;
		}

		public void LoadLevel(OpenLevel openLevel)
		{
			openLevel.Initialize();

		/*	if (Config.GetProperty("CalculateLights", false) && openLevel.WorldProvider is WrappedAnvilWorldProvider wawp)
			{
				wawp.Locked = true;

				//SkyLightCalculations.Calculate(openLevel);
				//RecalculateBlockLight(openLevel, wawp.AnvilProvider);

				wawp.Locked = false;
			}*/

			if (_levels.TryAdd(openLevel.LevelId, openLevel))
			{
				Levels.Add(openLevel);

				LevelInitEvent initEvent = new LevelInitEvent(openLevel);
				Api.EventDispatcher.DispatchEvent(initEvent);

				//OnLevelCreated?.Invoke(openLevel);

				Log.InfoFormat("Level loaded: {0}", openLevel.LevelId);
			}
			else
			{
				Log.Warn($"Failed to add level: {openLevel.LevelId}");
			}
		}

		public void UnloadLevel(OpenLevel openLevel)
		{
			OpenLevel l;
			if (_levels.TryRemove(openLevel.LevelId, out l))
			{
				Levels.Remove(openLevel);

					LevelClosedEvent levelClosed = new LevelClosedEvent(openLevel);
					openLevel.EventDispatcher.DispatchEvent(levelClosed);
				//OnLevelDestroyed?.Invoke(openLevel);
				openLevel.Close();
				Log.InfoFormat("Level destroyed: {0}", openLevel.LevelId);
			}
		}

		public override Level GetDimension(Level level, Dimension dimension)
		{
			return base.GetDimension(level, dimension);
		}

		public void SetDefaultLevel(OpenLevel level)
		{
			_defaultLevel = level.LevelId;
		}
	}
}
