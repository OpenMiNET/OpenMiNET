using System;

namespace OpenAPI.GameEngine.Models.Games
{
    public class GameAttribute : Attribute
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }

        public GameAttribute(string name, string author, string version)
        {
            Name = name;
            Author = author;
            Version = Version;
        }

        public GameAttribute(string name)
        {
            Name = name;
        }
    }
}