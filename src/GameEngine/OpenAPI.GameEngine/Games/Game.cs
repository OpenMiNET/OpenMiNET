using System;
using OpenAPI.GameEngine.Games.Configuration;
using OpenAPI.GameEngine.Games.Stages;
using OpenAPI.World;

namespace OpenAPI.GameEngine.Games
{
    public class Game : IDisposable
    {
        private IGameOwner Owner { get; }
        
        public GameConfig Config { get; private set; }
        
        public OpenLevel Level { get; private set; }
        
        private GameState _state = GameState.Created;
        public GameState State
        {
            get { return _state; }
            internal set
            {
                var oldState = _state;
                
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
        
        protected StageManager StageManager { get; }

        internal void Initialize()
        {
            OnInitialize();

            StageManager.Start();
            
            State = GameState.Ready;
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

        public void Dispose()
        {
            
        }
    }
}