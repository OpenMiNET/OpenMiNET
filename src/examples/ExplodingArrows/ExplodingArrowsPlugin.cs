using System;
using ExplodingArrows.Items;
using MiNET.Plugins.Attributes;
using OpenAPI;
using OpenAPI.Player;
using OpenAPI.Plugins;

namespace ExplodingArrows
{
    [OpenPluginInfo(Name = "Exploding Arrows")]
    public class ExplodingArrowsPlugin : OpenPlugin
    {
        /// <summary>
        /// 	The method that gets invoked as soon as a plugin gets Enabled.
        /// 	Any initialization should be done in here.
        /// </summary>
        /// <param name="api">An instance to OpenApi</param>
        public override void Enabled(OpenApi api)
        {
            api.CommandManager.LoadCommands(this);
        }

        /// <summary>
        /// 	The method that gets invoked as soon as a plugin gets Disabled.
        /// 	Any content initialized in <see cref="OpenPlugin.Enabled"/> should be de-initialized in here.
        /// </summary>
        /// <param name="api">An instance to OpenApi</param>
        public override void Disabled(OpenApi api)
        {
            api.CommandManager.UnloadCommands(this);
        }

        [Command(Name = "explosivebow", Description = "Gives a player a bow that when shot causes explosions")]
        public void GiveExplosionBow(OpenPlayer player)
        {
            player.Inventory.AddItem(new ExplodingArrowsBow(), true);
            player.SendMessage($"Enjoy!");
        }
    }
}