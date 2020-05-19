using System;
using OpenAPI.GameEngine.Games;
using OpenAPI.GameEngine.Games.Stages;
using OpenAPI.GameEngine.Games.Stages.Builtin;

namespace TNTRun.Stages
{
    public class RunningStage : TimedGameStage
    {
        public RunningStage(Game game) : base(game, TimeSpan.FromMinutes(5))
        {
            
        }
    }
}