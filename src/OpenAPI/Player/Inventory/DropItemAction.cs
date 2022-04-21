using log4net;
using MiNET.Items;
using OpenAPI.Events;
using OpenAPI.Events.Player;

namespace OpenAPI.Player.Inventory
{
	public class DropItemAction : InventoryAction
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(DropItemAction));
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
			//Log.Info($"Dropped item");
		}

		/// <inheritdoc />
		public override void ExecutionFailed(OpenPlayer source)
		{
			//Log.Warn($"Failed to drop item!");
		}
		
		/// <inheritdoc />
		public override string ToString()
		{
			return
				$"{{Action=DropItem SourceItem={SourceItem} TargetItem={TargetItem}}}";
		}
	}
}