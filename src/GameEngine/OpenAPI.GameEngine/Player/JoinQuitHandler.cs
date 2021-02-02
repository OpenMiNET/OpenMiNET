using OpenAPI.Events;
using OpenAPI.Events.Player;
using OpenAPI.GameEngine.Games;

namespace OpenAPI.GameEngine
{
    public class JoinQuitHandler : IEventHandler
    {
        private GameManager GameManager { get; }
        public JoinQuitHandler(GameManager gameManager)
        {
            GameManager = gameManager;
        }

        [EventHandler(EventPriority.Monitor)]
        public void OnPlayerJoin(PlayerJoinEvent e)
        {
            PlayerGameAttribute gameAttribute = new PlayerGameAttribute(e.Player, GameManager);
            e.Player.SetAttribute(gameAttribute);

            var defaultGame = GameManager.GetDefault();

            if (defaultGame != null && defaultGame.TryGetInstance(GameState.WaitingForPlayers, out var instance))
            {
                e.Player.SetGame(instance);
                return;
            }
            
            e.Player.Disconnect($"No game instance found for you to join!");
        }

        [EventHandler(EventPriority.Monitor, true)]
        public void OnPlayerQuit(PlayerQuitEvent e)
        {
            var attribute = e.Player.GetAttribute<PlayerGameAttribute>();

            if (attribute == null)
                return;

            attribute.PlayerQuit(e);
        }
    }
}