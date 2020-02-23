using MiNET.Items;
using OpenAPI.Player;

namespace OpenAPI.Events.Player
{
    /// <summary>
    ///     Dispatched when an <see cref="OpenPlayer"/> uses an Item
    /// </summary>
    public class PlayerItemUseEvent : PlayerEvent
    {
        /// <summary>
        ///     The item used
        /// </summary>
        public Item ItemUsed { get; } 
        public PlayerItemUseEvent(OpenPlayer player, Item itemUsed) : base(player)
        {
            ItemUsed = itemUsed;
        }
    }
}