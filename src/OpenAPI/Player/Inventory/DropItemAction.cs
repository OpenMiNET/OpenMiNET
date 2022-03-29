using MiNET.Items;
using OpenAPI.Events;
using OpenAPI.Events.Player;

namespace OpenAPI.Player.Inventory
{
	public class DropItemAction : InventoryAction
	{
		/// <inheritdoc />
		public DropItemAction(Item targetItem) : base(new ItemAir(), targetItem)
		{
			
		}

		/// <inheritdoc />
		public override bool IsValid(OpenPlayer source)
		{
			return TargetItem != null && TargetItem is not ItemAir && TargetItem.Count > 0;
		}

		/// <inheritdoc />
		public override bool PreExecute(OpenPlayer source)
		{
			PlayerItemDropEvent dropEvent = new PlayerItemDropEvent(source, source.KnownPosition, TargetItem, SourceItem);
			source.EventDispatcher.DispatchEvent(dropEvent);
			if (dropEvent.IsCancelled)
			{
				return false;
			}

			return true;
		}

		/// <inheritdoc />
		public override bool Execute(OpenPlayer source)
		{
			source.DropItem(TargetItem);

			return true;
		}

		/// <inheritdoc />
		public override void ExecutionSucceeded(OpenPlayer source)
		{
			
		}

		/// <inheritdoc />
		public override void ExecutionFailed(OpenPlayer source)
		{
			
		}
	}
}