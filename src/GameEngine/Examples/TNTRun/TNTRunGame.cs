using OpenAPI.GameEngine.Games;
using OpenAPI.GameEngine.Games.Stages.Builtin;
using TNTRun.Stages;

namespace TNTRun
{
    public class TNTRunGame : Game
    {
        public TNTRunGame(IGameOwner owner) : base(owner)
        {
            StageManager.Add(new LobbyGameStage(this));
            StageManager.Add(new RunningStage(this));
        }
    }
}