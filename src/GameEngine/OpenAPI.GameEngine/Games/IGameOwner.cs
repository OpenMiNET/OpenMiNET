namespace OpenAPI.GameEngine.Games
{
    public interface IGameOwner
    {
        void StateChanged(Game game, GameState oldState, GameState newState);
    }
}