using MiNET.Items;
using MiNET.Utils;
using OpenAPI.Player;

namespace OpenAPI.Events.Player
{
	public class PlayerItemDropEvent : PlayerEvent
	{
		public Item DroppedItem { get; }
		public Item NewInventoryItem { get; }
		public PlayerLocation From { get; }
		public PlayerItemDropEvent(OpenPlayer player, PlayerLocation dropFrom, Item droppedItem, Item newInventoryItem) : base(player)
		{
			DroppedItem = droppedItem;
			NewInventoryItem = newInventoryItem;
			From = dropFrom;
		}
	}
}
