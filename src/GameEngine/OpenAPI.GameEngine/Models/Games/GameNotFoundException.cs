using System;

namespace OpenAPI.GameEngine.Models.Games
{
    public class GameNotFoundException : Exception
    {
        public GameNotFoundException(string gameName) : base($"No game was found with the name \"{gameName}\"")
        {
            
        }
    }
}