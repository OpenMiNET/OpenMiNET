using System;

namespace OpenAPI.GameEngine.Models.Games
{
    public class InvalidGameStateException : Exception
    {
        public InvalidGameStateException(string message) : base(message)
        {
            
        }
    }
}