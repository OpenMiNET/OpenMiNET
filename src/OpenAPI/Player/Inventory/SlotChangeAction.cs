using MiNET;
using MiNET.Items;

namespace OpenAPI.Player.Inventory
{
	public class SlotChangeAction : InventoryAction
	{
		private readonly OpenPlayer _player;
		public readonly int InventoryId;
		public readonly int InventorySlot;

		/// <inheritdoc />
		public SlotChangeAction(OpenPlayer player, int inventoryId, int inventorySlot, Item sourceItem, Item targetItem) : base(sourceItem, targetItem)
		{
			_player = player;
			InventoryId = inventoryId;
			InventorySlot = inventorySlot;
		}

		/// <inheritdoc />
		public override bool IsValid(OpenPlayer source)
		{
			return InventorySlot >= 0 && InventorySlot < _player.Inventory.Slots.Count && EqualsExactly(_player.GetInvItem(InventoryId, InventorySlot),  SourceItem);
		}

		/// <inheritdoc />
		public override bool Execute(OpenPlayer source)
		{
			_player.SetInvItem(InventoryId, InventorySlot, TargetItem);
			//_player.Slots[_inventorySlot] = TargetItem;
			return true;
		}

		/// <inheritdoc />
		public override void ExecutionSucceeded(OpenPlayer source)
		{
			//_player.SendSetSlot(_inventorySlot);
		}

		/// <inheritdoc />
		public override void ExecutionFailed(OpenPlayer source)
		{
			_player.SetInvItem(InventoryId, InventorySlot, SourceItem);
		}

		public static bool EqualsExactly(Item a, Item b)
		{
			return a.Equals(b) && a.Count == b.Count;
			return a.Id == b.Id && a.Metadata == b.Metadata && a.ExtraData == b.ExtraData && a.Count == b.Count;
		}
	}
}