using MiNET.Items;
using MiNET.Utils;
using MiNET.Utils.Vectors;
using OpenAPI.Player;

namespace OpenAPI.Events.Player
{
	/// <summary>
	/// 	Dispatched when an <see cref="OpenPlayer"/> drops an item
	/// </summary>
	public class PlayerItemDropEvent : PlayerEvent
	{
		/// <summary>
		/// 	The item dropped
		/// </summary>
		public Item DroppedItem { get; }
		
		/// <summary>
		/// 	The item replacing the inventory slot
		/// </summary>
		public Item NewInventoryItem { get; }
		
		/// <summary>
		/// 	The location of the player at the time of dropping
		/// </summary>
		public PlayerLocation From { get; }
		public PlayerItemDropEvent(OpenPlayer player, PlayerLocation dropFrom, Item droppedItem, Item newInventoryItem) : base(player)
		{
			DroppedItem = droppedItem;
			NewInventoryItem = newInventoryItem;
			From = dropFrom;
		}
	}
}
