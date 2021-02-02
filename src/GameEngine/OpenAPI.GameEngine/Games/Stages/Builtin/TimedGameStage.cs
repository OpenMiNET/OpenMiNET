using System;
using System.Diagnostics;
using System.Linq;
using MiNET;

namespace OpenAPI.GameEngine.Games.Stages.Builtin
{
    public class TimedGameStage : GameStage
    {
        private TimeSpan RunTime       { get; }
        public  TimeSpan TimeRemaining => RunTime - _timer.Elapsed;
        public TimedGameStage(Game game, TimeSpan runTime, string identifier) : base(game, identifier)
        {
            RunTime = runTime;
        }

        private Stopwatch _timer;
        protected override void OnStart()
        {
            _timer = Stopwatch.StartNew();
        }

        protected override void OnTick()
        {
            if (StageTicks % 20 == 0)
            {
                var remaining = RunTime - _timer.Elapsed;
                Game.Level.BroadcastMessage($"Remaining: {remaining.Minutes}:{remaining.Seconds}", MessageType.Popup);
            }
        }

        public override bool ShouldFinish()
        {
            return _timer.Elapsed >= RunTime || !Game.Players.Any();
        }

        protected override void OnFinish()
        {
            base.OnFinish();
            _timer.Stop();
        }
    }
}