namespace OpenAPI.GameEngine.Games.Stages
{
    public class GameStage
    {
        public GameStage()
        {
            
        }

        internal void Tick()
        {
            OnTick();
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
    }
}