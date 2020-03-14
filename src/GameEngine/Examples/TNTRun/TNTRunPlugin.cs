using System;
using OpenAPI;
using OpenAPI.GameEngine;
using OpenAPI.Plugins;

namespace TNTRun
{
    [OpenPluginInfo(Name = "TNTRun", Author = "Kenny van Vulpen", Description = "A TNTRun gamemode for the OpenAPI GameEngine")]
    public class TNTRunPlugin : OpenPlugin
    {
        private GameEngine Engine { get; }
        
        public TNTRunPlugin(GameEngine gameEngine)
        {
            Engine = gameEngine;
        }
        
        /// <summary>
        /// 	The method that gets invoked as soon as a plugin gets Enabled.
        /// 	Any initialization should be done in here.
        /// </summary>
        /// <param name="api">An instance to OpenApi</param>
        public override void Enabled(OpenApi api)
        {
            Engine.GameManager.RegisterGame<TNTRunGame>((owner) => new TNTRunGame(owner));
            Engine.GameManager.SetDefault<TNTRunGame>();
        }

        /// <summary>
        /// 	The method that gets invoked as soon as a plugin gets Disabled.
        /// 	Any content initialized in <see cref="OpenPlugin.Enabled"/> should be de-initialized in here.
        /// </summary>
        /// <param name="api">An instance to OpenApi</param>
        public override void Disabled(OpenApi api)
        {
            
        }
    }
}