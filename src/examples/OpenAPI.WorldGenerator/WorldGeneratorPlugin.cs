using System;
using System.Threading;
using log4net;
using MiNET;
using MiNET.Utils;
using MiNET.Worlds;
using Newtonsoft.Json;
using OpenAPI.Entities;
using OpenAPI.Events;
using OpenAPI.Events.Player;
using OpenAPI.Plugins;
using OpenAPI.World;
using OpenAPI.WorldGenerator.Generators;
using BiomeUtils = OpenAPI.WorldGenerator.Utils.BiomeUtils;

namespace OpenAPI.WorldGenerator
{
    public class WorldGeneratorPlugin : OpenPlugin, IEventHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WorldGeneratorPlugin));

        private OpenAPI Api { get; set; }
        private Timer _timer { get; set; }
      //  private WorldGeneratorPreset Preset { get; }
        public WorldGeneratorPlugin()
        {
        }

        private void Callback(object state)
        {
            if (Api == null) return;
            foreach (var player in Api.PlayerManager.GetPlayers())
            {
                if (!player.IsSpawned)
                    continue;
                
                var pos = player.KnownPosition.GetCoordinates3D();
                var chunk = player.Level.GetChunk(pos, true);

                if (chunk == null)
                    continue;
                
                var biome = chunk.GetBiome(pos.X - (chunk.X * 16), pos.Z - (chunk.Z * 16));
                var result = BiomeUtils.GetBiomeById(biome);
                
                player.SendTitle($"Biome: {result.Name} | ", TitleType.ActionBar, 0, 0, 25);
            }
        }

        public override void Enabled(OpenAPI api)
        {
            Api = api;
            var level = new OpenLevel(api, api.LevelManager, Dimension.Overworld.ToString(), new DebugWorldProvider(new OverworldGeneratorV2()), api.LevelManager.EntityManager, GameMode.Creative, Difficulty.Peaceful);

            api.LevelManager.LoadLevel(level);
            
            api.LevelManager.SetDefaultLevel(level);
            
            api.EventDispatcher.RegisterEvents(this);
            
            _timer = new Timer(Callback, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            
            Log.Info($"WorldGenerator plugin enabled!");
        }

        public override void Disabled(OpenAPI api)
        {
            Log.Info($"WorldGenerator plugin disabled!");
        }
        

        [EventHandler(EventPriority.Highest)]
        public void OnPlayerLoginComplete(PlayerSpawnedEvent e)
        {
            var lvl = Api.LevelManager.GetLevel(e.Player, Dimension.Overworld.ToString());
            
            e.Player.SpawnLevel(lvl, lvl.SpawnPoint, false, null, () =>
            {
                e.Player.SetGameMode(GameMode.Creative);
                e.Player.Teleport(new PlayerLocation(e.Player.KnownPosition.X, e.Player.Level.GetHeight(e.Player.KnownPosition.GetCoordinates3D()) + 3f, e.Player.KnownPosition.Z));
            });
        }
    }
}