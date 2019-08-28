using MiNET.Items;
using OpenAPI.Player;

namespace OpenAPI.Events.Player
{
    public class PlayerItemUseEvent : PlayerEvent
    {
        public Item ItemUsed { get; } 
        public PlayerItemUseEvent(OpenPlayer player, Item itemUsed) : base(player)
        {
            ItemUsed = itemUsed;
        }
    }
}