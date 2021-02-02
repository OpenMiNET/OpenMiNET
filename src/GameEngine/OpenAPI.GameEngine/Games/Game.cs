using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MiNET.Utils;
using Newtonsoft.Json;
using OpenAPI.GameEngine.Games.Configuration;
using OpenAPI.GameEngine.Games.Stages;
using OpenAPI.GameEngine.Games.Teams;
using OpenAPI.Player;
using OpenAPI.World;
using MapInfo = OpenAPI.GameEngine.Models.Games.Maps.MapInfo;

namespace OpenAPI.GameEngine.Games
{
    public class Game : IDisposable
    {
        private IGameOwner Owner { get; }
        
        public GameConfig<MapInfo> Config { get; private set; }
        public MapInfo Map { get; internal set; }
        
        public OpenLevel Level { get; internal set; }
        
        private GameState _state = GameState.Created;
        public GameState State
        {
            get { return _state; }
            internal set
            {
                var oldState = _state;

                if (oldState == value)
                    return;
                
                _state = value;
                
                Owner.StateChanged(this, oldState, _state);
            }
        }

        internal string InstanceName { get; set; } = null;
        
        protected Game(IGameOwner owner)
        {
            Owner = owner;

            StageManager = new StageManager(this);
        }
        
        public TeamManager TeamManager { get; private set; }
        protected StageManager StageManager { get; }

        public IEnumerable<OpenPlayer> Players => TeamManager.GetTeams().SelectMany(x => x.Players);

        internal void Load()
        {
            LoadContent();
            LoadConfig();
        }
        
        internal void Initialize()
        {
            State = GameState.Initializing;
            
            TeamManager = new TeamManager(Config.Teams);
            
            OnInitialize();

            StageManager.Start();
            
            State = GameState.Ready;
        }

        private void LoadConfig()
        {
            if (Config != null)
                return;
            
            Config = new GameConfig<MapInfo>();
            
            string gameAssemblyPath = Path.GetDirectoryName(GetType().Assembly.Location);
            string configPath = Path.Combine(gameAssemblyPath, "game.json");

            if (File.Exists(configPath))
            {
                var content = File.ReadAllText(configPath);
                Config = JsonConvert.DeserializeObject<GameConfig<MapInfo>>(content);
                //Config = JsonConvert.DeserializeObject<GameConfig>(content);
            }
        }

        protected virtual void LoadContent()
        {
            
        }
        
        protected virtual void OnInitialize()
        {
            
        }

        internal void Tick()
        {
            OnTick();

            if (!StageManager.Tick())
            {
                //No stage left to tick, we should end the game.
                
            }
        }

        protected virtual void OnTick()
        {
            
        }

        internal void LeaveGame(OpenPlayer player)
        {
            player.GetGameAttribute().Team?.Leave(player);
        }

        internal bool TryJoin(OpenPlayer player)
        {
            if (!CanJoin())
                return false;

            if (TeamManager.TryAssignTeam(player))
            {
                PlayerLocation spawnPosition;

                if (!TryGetSpawnPosition(out spawnPosition))
                    spawnPosition = Level.SpawnPoint;
                
                player.SpawnLevel(Level, spawnPosition, false);
                return true;
            }

            return false;
        }

        protected virtual bool CanJoin()
        {
            return State == GameState.WaitingForPlayers;
        }

        internal bool TryGetSpawnPosition(out PlayerLocation spawn)
        {
            if (Map?.Stages != null && Map.Stages.TryGetValue(StageManager.CurrentStage.Identifier, out var configuration))
            {
                if (configuration.Spawn != null)
                {
                    spawn = configuration.Spawn;
                    return true;
                }
            }

            spawn = null;
            return false;
        }
        
        public void Dispose()
        {
            
        }
    }
}