using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using OpenAPI.GameEngine.Games.Configuration;
using OpenAPI.GameEngine.Games.Stages;
using OpenAPI.GameEngine.Games.Teams;
using OpenAPI.GameEngine.Models.Games.Maps;
using OpenAPI.Player;
using OpenAPI.World;

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
            
            StageManager.Tick();
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
                player.SpawnLevel(Level, Level.SpawnPoint, false);
                return true;
            }

            return false;
        }

        protected virtual bool CanJoin()
        {
            return State == GameState.WaitingForPlayers;
        }
        
        public void Dispose()
        {
            
        }
    }
}