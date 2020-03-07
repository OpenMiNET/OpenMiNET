using System.Collections.Concurrent;

namespace OpenAPI.GameEngine.Games.Stages
{
    public class StageManager
    {
        private Game Game { get; }
        
        private GameStage Current { get; set; }
        private ConcurrentQueue<GameStage> Stages { get; set; }
        
        public StageManager(Game game)
        {
            Game = game;
            Current = null;
            Stages = new ConcurrentQueue<GameStage>();
        }

        public void Start()
        {
            Next();
        }

        public bool Next()
        {
            if (Stages.TryDequeue(out var stage))
            {
                Current?.Finish();
                Current = stage;
                return true;
            }

            return false;
        }
        
        public void Tick()
        {
            var stage = Current;
            if (stage == null)
                return;
            
            stage.Tick();

            if (stage.ShouldFinish())
            {
                Next();
            }
        }

        public void Add(GameStage stage)
        {
            Stages.Enqueue(stage);
        }
    }
}