namespace OpenAPI.GameEngine.Games.Stages
{
    public class GameStage
    {
        protected Game Game { get; }
        protected long StageTicks { get; private set; }
        public GameStage(Game game)
        {
            Game = game;
        }

        internal void Tick()
        {
            OnTick();
            StageTicks++;
        }

        protected virtual void OnTick()
        {
            
        }

        internal virtual bool ShouldFinish()
        {
            return false;
        }

        internal void Finish()
        {
            OnFinish();
        }

        protected virtual void OnFinish()
        {
            
        }

        internal void Start()
        {
            OnStart();
        }

        protected virtual void OnStart()
        {
            
        }
    }
}