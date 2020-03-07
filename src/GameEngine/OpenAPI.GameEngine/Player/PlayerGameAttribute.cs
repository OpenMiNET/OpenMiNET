using OpenAPI.Events.Player;
using OpenAPI.GameEngine.Games;
using OpenAPI.GameEngine.Games.Teams;
using OpenAPI.Player;

namespace OpenAPI.GameEngine
{
    public class PlayerGameAttribute : IOpenPlayerAttribute
    {
        private GameManager GameManager { get; }
        private OpenPlayer Player { get; }
        
        public Team Team { get; internal set; } = null;
        public Game Game { get; private set; } = null;
        
        public PlayerGameAttribute(OpenPlayer player, GameManager gameManager)
        {
            Player = player;
            GameManager = gameManager;
        }
        
        public bool InGame => Team != null && Game != null;

        public void JoinGame(Game game)
        {
            var oldGame = Game;
            oldGame?.LeaveGame(Player);
            
            if (game.TryJoin(Player))
            {
                Game = game;
            }
        }
        
        internal void PlayerQuit(PlayerQuitEvent playerQuitEvent)
        {
            if (Game != null)
                Game.LeaveGame(Player);
        }
    }
}