using OpenAPI.Events;

namespace OpenAPI.GameEngine.Games.Stages
{
    public class GameStage : IEventHandler
    {
        protected Game                   Game          { get; }
        protected long                   StageTicks    { get; private set; }
        public string                 Identifier    { get; private set; }
        public GameStage(Game game, string identifier)
        {
            Game = game;
            Identifier = identifier;
        }

        internal void Tick()
        {
            OnTick();
            StageTicks++;
        }

        protected virtual void OnTick()
        {
            
        }

        public virtual bool ShouldFinish()
        {
            return false;
        }

        internal void Finish()
        {
            Game.Level.EventDispatcher.UnregisterEvents(this);
            OnFinish();
        }

        protected virtual void OnFinish()
        {
            
        }

        internal void Start()
        {
            Game.Level.EventDispatcher.RegisterEvents(this);
            
            Load();
            OnStart();
        }

        private void Load()
        {
            
        }

        protected virtual void OnStart()
        {
            
        }
    }
}