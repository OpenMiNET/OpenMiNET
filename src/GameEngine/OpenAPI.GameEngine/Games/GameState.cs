namespace OpenAPI.GameEngine.Games
{
    public enum GameState
    {
        Created = 0,
        Initializing = 1,
        Ready = 2,
        WaitingForPlayers = 3,
        Starting = 4,
        InProgress = 5,
        Finished = 6,
        Empty = 7
    }
}