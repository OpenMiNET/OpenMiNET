using System;
using log4net;
using OpenAPI.Plugins;

namespace OpenAPI.GameEngine
{
    [OpenPluginInfo(Name = "OpenAPI GameEngine", Description = "A free to use Game Engine for OpenAPI",
        Author = "Kenny van Vulpen")]
    public class GameEngine : OpenPlugin
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GameEngine));

        private OpenApi Host { get; set; }

        public GameEngine()
        {
            
        }
        
        public override void Enabled(OpenApi api)
        {
            Host = api;
        }

        public override void Disabled(OpenApi api)
        {

        }
    }
}