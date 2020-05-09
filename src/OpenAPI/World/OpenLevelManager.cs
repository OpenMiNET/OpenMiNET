using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Core;
using MiNET;
using MiNET.Utils;
using MiNET.Worlds;
using OpenAPI.Entities;
using OpenAPI.Events;
using OpenAPI.Events.Level;
using OpenAPI.Utils;
using Level = MiNET.Worlds.Level;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace OpenAPI.World
{
    /// <summary>
    /// 	The LevelManager keeps track of all available Levels (A.K.A Worlds) in the server.
    /// </summary>
    public class OpenLevelManager : LevelManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(OpenLevelManager));

        public BiomeManager BiommManager = new BiomeManager();

        private OpenApi Api { get; }

        private readonly ConcurrentDictionary<string, OpenLevel>
            _levels = new ConcurrentDictionary<string, OpenLevel>();

        public int LevelCount => _levels.Count;
        public bool HasDefaultLevel => _defaultLevelSet;

        private readonly GameMode _gameMode = Config.GetProperty("GameMode", GameMode.Survival);
        private readonly Difficulty _difficulty = Config.GetProperty("Difficulty", Difficulty.Normal);
        private readonly int _viewDistance = Config.GetProperty("ViewDistance", 11);
        private readonly bool _enableBlockTicking = Config.GetProperty("EnableBlockTicking", false);
        private readonly bool _enableChunkTicking = Config.GetProperty("EnableChunkTicking", false);
        private readonly bool _isWorldTimeStarted = Config.GetProperty("IsWorldTimeStarted", false);
        private readonly bool _readSkyLight = !Config.GetProperty("CalculateLights", false);
        private readonly bool _readBlockLight = !Config.GetProperty("CalculateLights", false);

        private string _defaultLevel = "Overworld";

        private bool _defaultLevelSet = false;

        //public new LevelCreated OnLevelCreated = null;
        //public LevelDestroyed OnLevelDestroyed = null;
        public OpenLevelManager(OpenApi api)
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

            /*var worldProvider = new WrappedAnvilWorldProvider(Api)
            {
                MissingChunkProvider = new FlatlandWorldProvider(),
                ReadSkyLight = _readSkyLight,
                ReadBlockLight = _readBlockLight
            };*/
            var worldProvider = new AnvilWorldProvider();

            var openLevel = new OpenLevel(Api /*, Api.EventDispatcher*/, this, newLevelid, worldProvider, EntityManager,
                _gameMode, _difficulty, _viewDistance)
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

            /*var worldProvider = new WrappedAnvilWorldProvider(Api, basepath, false, chunks)
            {
                MissingChunkProvider = new FlatlandWorldProvider(),
                ReadSkyLight = _readSkyLight,
                ReadBlockLight = _readBlockLight
            };*/
            var worldProvider = new AnvilWorldProvider(basepath);
            var openLevel = new OpenLevel(Api /*, Api.EventDispatcher*/, this, newLevelId, worldProvider, EntityManager,
                _gameMode, _difficulty, _viewDistance)
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

            /*	var worldProvider = new WrappedAnvilWorldProvider(Api, levelDirectory)
                {
                    MissingChunkProvider = new FlatlandWorldProvider(),
                    ReadSkyLight = !Config.GetProperty("CalculateLights", false),
                    ReadBlockLight = !Config.GetProperty("CalculateLights", false),
                };*/
            Log.Error("====================================");
            Log.Error("====================================");
            Log.Error("====================================");
            Log.Error("====================================");
            Log.Error("====================================");
            Log.Error("====================================");
            Log.Error("====================================");
            IWorldProvider worldProvider;
            if (debug)
            {
                 worldProvider = new OpenExperimentalWorldProvider(123123);
            }else
            {
                 worldProvider = new AnvilWorldProvider(levelDirectory)
                {
                    MissingChunkProvider = new TestGenerator2(Dimension.Overworld)
                    // MissingChunkProvider = new SuperflatGenerator(Dimension.Overworld)
                };
            }

            var openLevel = new OpenLevel(Api /*, Api.EventDispatcher*/, this, newLevelId, worldProvider, EntityManager,
                _gameMode, _difficulty, _viewDistance)
            {
                EnableBlockTicking = _enableBlockTicking,
                EnableChunkTicking = _enableChunkTicking,
                //	IsWorldTimeStarted = _isWorldTimeStarted
            };

            LoadLevel(openLevel);

            return openLevel;
        }

        /// <summary>
        /// 	Initializes the <see cref="OpenLevel"/> instance, this could include loading the world from a local folder or generating a new world.
        /// </summary>
        /// <param name="openLevel">The <see cref="OpenLevel"/> instance to register and initialize</param>
        public void LoadLevel(OpenLevel openLevel)
        {
            openLevel.Initialize();

            if (openLevel.WorldProvider is OpenExperimentalWorldProvider)
            {
                var o = (OpenExperimentalWorldProvider)openLevel.WorldProvider;
                o.Level = openLevel;
            }
            
            if (Config.GetProperty("CalculateLights", false) && openLevel.WorldProvider is WrappedWorldProvider wawp &&
                wawp.WorldProvider is AnvilWorldProvider anvilWorldProvider)
            {
                anvilWorldProvider.Locked = true;

                SkyLightCalculations.Calculate(openLevel);
                RecalculateBlockLight(openLevel, anvilWorldProvider);

                anvilWorldProvider.Locked = false;
            }

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

        /// <summary>
        /// 	Unloads & unregisters a level from the current <see cref="OpenLevelManager"/>
        /// </summary>
        /// <param name="openLevel">The level to unload</param>
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

        /// <summary>
        /// 	Sets the Default <see cref="OpenLevel"/> players join when connecting to the server.
        /// </summary>
        /// <param name="level"></param>
        public void SetDefaultLevel(OpenLevel level)
        {
            _defaultLevel = level.LevelId;
            _defaultLevelSet = true;

            if (!_levels.ContainsKey(level.LevelId))
            {
                LoadLevel(level);
            }
        }

        public OpenLevel GetDefaultLevel()
        {
            return (OpenLevel) GetLevel(null, _defaultLevel);
        }

            bool debug = true;
        internal void SetDefaultByConfig()
        {
            var missingGenerator = new TestGenerator2(Dimension.Overworld);
            IWorldProvider worldProvider;
            if (debug)
            {
                worldProvider = new OpenExperimentalWorldProvider(123123);
            }
            else
                switch (Config.GetProperty("WorldProvider", "anvil").ToLower().Trim())
                {
                    case "leveldb":
                        worldProvider = new LevelDbProvider()
                        {
                            MissingChunkProvider = missingGenerator
                        };
                        break;
                    case "anvil":
                    default:
                        worldProvider = new AnvilWorldProvider()
                        {
                            MissingChunkProvider = missingGenerator,
                            ReadSkyLight = !Config.GetProperty("CalculateLights", false),
                            ReadBlockLight = !Config.GetProperty("CalculateLights", false)
                        };
                        break;
                }

            var lvl = new OpenLevel(Api, this, Dimension.Overworld.ToString(), worldProvider, EntityManager, _gameMode,
                _difficulty, _viewDistance)
            {
                EnableBlockTicking = Config.GetProperty("EnableBlockTicking", false),
                EnableChunkTicking = Config.GetProperty("EnableChunkTicking", false),
                SaveInterval = Config.GetProperty("Save.Interval", 300),
                UnloadInterval = Config.GetProperty("Unload.Interval", -1),

                DrowningDamage = Config.GetProperty("GameRule.DrowningDamage", true),
                CommandblockOutput = Config.GetProperty("GameRule.CommandblockOutput", true),
                DoTiledrops = Config.GetProperty("GameRule.DoTiledrops", true),
                DoMobloot = Config.GetProperty("GameRule.DoMobloot", true),
                KeepInventory = Config.GetProperty("GameRule.KeepInventory", true),
                DoDaylightcycle = Config.GetProperty("GameRule.DoDaylightcycle", true),
                DoMobspawning = Config.GetProperty("GameRule.DoMobspawning", true),
                DoEntitydrops = Config.GetProperty("GameRule.DoEntitydrops", true),
                DoFiretick = Config.GetProperty("GameRule.DoFiretick", true),
                DoWeathercycle = Config.GetProperty("GameRule.DoWeathercycle", true),
                Pvp = Config.GetProperty("GameRule.Pvp", true),
                Falldamage = Config.GetProperty("GameRule.Falldamage", true),
                Firedamage = Config.GetProperty("GameRule.Firedamage", true),
                Mobgriefing = Config.GetProperty("GameRule.Mobgriefing", true),
                ShowCoordinates = Config.GetProperty("GameRule.ShowCoordinates", true),
                NaturalRegeneration = Config.GetProperty("GameRule.NaturalRegeneration", true),
                TntExplodes = Config.GetProperty("GameRule.TntExplodes", true),
                SendCommandfeedback = Config.GetProperty("GameRule.SendCommandfeedback", true),
                RandomTickSpeed = Config.GetProperty("GameRule.RandomTickSpeed", 3)
            };

            SetDefaultLevel((OpenLevel) lvl);
        }
    }
}