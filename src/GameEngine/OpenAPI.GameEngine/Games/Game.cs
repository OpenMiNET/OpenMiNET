using OpenAPI.GameEngine.Games.Configuration;

namespace OpenAPI.GameEngine.Games
{
    public class Game
    {
        private IGameOwner Owner { get; }
        
        public GameConfig Config { get; private set; }
        public GameState State { get; private set; } = GameState.Intializing;
        
        protected Game(IGameOwner owner)
        {
            Owner = owner;
        }
    }
}