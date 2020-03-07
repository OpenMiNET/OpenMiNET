using System;

namespace OpenAPI.GameEngine.Models.Games
{
    public class DuplicateGameInstanceException : Exception
    {
        public DuplicateGameInstanceException(string message) : base(message)
        {
            
        }
    }
}