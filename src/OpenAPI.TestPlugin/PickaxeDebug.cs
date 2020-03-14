using MiNET.Items;
using MiNET.Worlds;
using OpenAPI.Events;
using OpenAPI.Events.Player;
using OpenAPI.Plugins;

namespace OpenAPI.TestPlugin
{
    [OpenPluginInfo(Name = "Pickaxe debugging")]
    public class PickaxeDebug : OpenPlugin, IEventHandler
    {
        /// <summary>
        /// 	The method that gets invoked as soon as a plugin gets Enabled.
        /// 	Any initialization should be done in here.
        /// </summary>
        /// <param name="api">An instance to OpenApi</param>
        public override void Enabled(OpenApi api)
        {
            api.EventDispatcher.RegisterEvents(this);
        }

        /// <summary>
        /// 	The method that gets invoked as soon as a plugin gets Disabled.
        /// 	Any content initialized in <see cref="OpenPlugin.Enabled"/> should be de-initialized in here.
        /// </summary>
        /// <param name="api">An instance to OpenApi</param>
        public override void Disabled(OpenApi api)
        {
            api.EventDispatcher.UnregisterEvents(this);
        }

        [EventHandler]
        public void OnPlayerSpawn(PlayerSpawnedEvent e)
        {
            e.Player.SetGamemode(GameMode.Survival);
            e.Player.Inventory.AddItem(new ItemDiamondPickaxe(), true);
            e.Player.Inventory.AddItem(new ItemGoldenPickaxe(), true);
            e.Player.Inventory.AddItem(new ItemStonePickaxe(), true);
            e.Player.Inventory.AddItem(new ItemWoodenPickaxe(), true);
        }
    }
}