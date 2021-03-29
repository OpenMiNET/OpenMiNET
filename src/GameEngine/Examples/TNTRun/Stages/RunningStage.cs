using System;
using System.Linq;
using MiNET;
using MiNET.Blocks;
using MiNET.Entities.World;
using MiNET.Utils;
using MiNET.Utils.Vectors;
using MiNET.Worlds;
using OpenAPI.Events;
using OpenAPI.Events.Entity;
using OpenAPI.Events.Player;
using OpenAPI.GameEngine;
using OpenAPI.GameEngine.Games;
using OpenAPI.GameEngine.Games.Stages.Builtin;
using OpenAPI.Player;

namespace TNTRun.Stages
{
    public class RunningStage : TimedGameStage
    {
        public RunningStage(Game game) : base(game, TimeSpan.FromMinutes(5), "running")
        {
            
        }

        /// <inheritdoc />
        protected override void OnStart()
        {
            base.OnStart();

            PlayerLocation spawnPoint = null;
            if (Game.Map?.Stages != null && Game.Map.Stages.TryGetValue(Identifier, out var stageConfiguration))
            {
                if (stageConfiguration.Spawn != null)
                    spawnPoint = stageConfiguration.Spawn;
            }

            if (spawnPoint == null)
            {
                spawnPoint = Game.Level.SpawnPoint;
                spawnPoint.Y = 65;
            }

            Game.TeamManager.ForEach(
                (team) =>
                {
                    team.TeleportTo(spawnPoint);
                });
        }

        private int _alivePlayerCount = 0;
        /// <inheritdoc />
        protected override void OnTick()
        {
            _alivePlayerCount = Game.Players.Count(x => x.GameMode != GameMode.Spectator && !x.HealthManager.IsDead);
            if (StageTicks % 20 == 0)
            {
                Game.Level.BroadcastMessage(
                    $"Players remaining: {_alivePlayerCount} | {TimeRemaining.Minutes}:{TimeRemaining.Seconds}",
                    MessageType.Popup);
            }
        }

        /// <inheritdoc />
        public override bool ShouldFinish()
        {
            return base.ShouldFinish() || _alivePlayerCount <= 0;
        }

        /// <inheritdoc />
        protected override void OnFinish()
        {
            var alivePlayers =
                Game.Players.Where(x => x.GameMode != GameMode.Spectator).ToArray();
            
            var deadPlayers =
                Game.Players.Where(x => x.GameMode == GameMode.Spectator).ToArray();

            foreach (var alivePlayer in alivePlayers)
            {
                alivePlayer.SendMessage($"{ChatColors.Gold}You won the game!");
            }
            
            foreach (var alivePlayer in deadPlayers)
            {
                alivePlayer.SendMessage($" {ChatColors.Gold}You lost!");
            }

            base.OnFinish();
        }

        private void DestroyBlock(Block block)
        {
            Game.Level.SetBlock(new Air()
            {
                Coordinates = block.Coordinates
            });
        }

        [EventHandler]
        public void OnPlayerMove(PlayerMoveEvent e)
        {
            if (e.Player.HealthManager.IsDead || e.Player.GameMode == GameMode.Spectator)
                return;

            var pos        = new BlockCoordinates(e.Player.KnownPosition);
           // pos.Y -= 1;
            
            var blockBelow = Game.Level.GetBlock(pos);

            if (blockBelow.IsSolid)
            {
                DestroyBlock(blockBelow);
            }
        }

        [EventHandler]
        public void OnPlayerDeath(EntityKilledEvent e)
        {
            if (e.Entity is OpenPlayer player)
            {
                player.SetGamemode(GameMode.Spectator);
                player.Teleport(player.SpawnPosition);
            }
        }
    }
}