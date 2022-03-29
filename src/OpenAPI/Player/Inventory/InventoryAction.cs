using MiNET.Items;

namespace OpenAPI.Player.Inventory
{
	public abstract class InventoryAction
	{
		/// <summary>
		///		 Returns the item that was present before the action took place.
		/// </summary>
		public Item SourceItem { get; }
		
		/// <summary>
		///		Returns the item that the action attempted to replace the source item with.
		/// </summary>
		public Item TargetItem { get; }

		protected InventoryAction(Item sourceItem, Item targetItem)
		{
			SourceItem = sourceItem;
			TargetItem = targetItem;
		}

		/// <summary>
		///		 Returns whether this action is currently valid. This should perform any necessary sanity checks.
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public abstract bool IsValid(OpenPlayer source);

		/// <summary>
		///  Called when the action is added to the specified InventoryTransaction.
		/// </summary>
		public virtual void OnAddToTransaction()
		{
			
		}

		/// <summary>
		/// Called by inventory transactions before any actions are processed. If this returns false, the transaction will be cancelled.
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public virtual bool PreExecute(OpenPlayer source)
		{
			return true;
		}

		
		/// <summary>
		/// Performs actions needed to complete the inventory-action server-side. Returns if it was successful.
		/// Will return false if plugins cancelled events.
		/// This will only be called if the transaction which it is part of is considered * valid.
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public abstract bool Execute(OpenPlayer source);

		/// <summary>
		///		Performs additional actions when this inventory-action completed successfully.
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public abstract void ExecutionSucceeded(OpenPlayer source);
		
		/// <summary>
		///		Performs additional actions when this inventory-action did not complete successfully.
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public abstract void ExecutionFailed(OpenPlayer source);
	}
}