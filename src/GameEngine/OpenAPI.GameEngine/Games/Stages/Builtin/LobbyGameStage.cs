using System;
using System.Diagnostics;
using MiNET;
using MiNET.Utils;

namespace OpenAPI.GameEngine.Games.Stages.Builtin
{
    public class LobbyGameStage : GameStage
    {
        private TimeSpan TimeToStart { get; set; } = TimeSpan.FromSeconds(5);
        public LobbyGameStage(Game game) : base(game, "PreGame")
        {
            
        }

        private Stopwatch _startTimer = new Stopwatch();
        protected override void OnTick()
        {
            if (Game.State == GameState.WaitingForPlayers || Game.State == GameState.Starting)
            {
                if (Game.TeamManager.CanStart())
                {
                    if (Game.State != GameState.Starting)
                    {
                        _startTimer.Restart();
                        Game.State = GameState.Starting;
                    }
                }
                else
                {
                    Game.State = GameState.WaitingForPlayers;
                }
            }

            if (StageTicks % 20 == 0)
            {
                if (Game.State == GameState.Starting)
                {
                    var remaining = (TimeToStart - _startTimer.Elapsed);

                    Game.Level.BroadcastMessage(
                        $"{ChatColors.Gold} Starting in {ChatColors.Red} {(int) remaining.TotalSeconds} seconds...",
                        MessageType.Popup);
                }
                else
                {
                    Game.Level.BroadcastMessage($"{ChatColors.Gold} Waiting for players...", MessageType.Popup);
                }
            }
            
            base.OnTick();
        }

        public override bool ShouldFinish()
        {
            return Game.State == GameState.Starting && Game.TeamManager.CanStart() &&
                   _startTimer.Elapsed >= TimeToStart;
        }

        protected override void OnFinish()
        {
            Game.State = GameState.InProgress;
            _startTimer.Stop();
        }
    }
}