using MiNET.Items;

namespace OpenAPI.Player.Inventory
{
	public class DestroyItemAction : InventoryAction
	{
		/// <inheritdoc />
		public DestroyItemAction(Item targetItem) : base(new ItemAir(), targetItem) { }

		/// <inheritdoc />
		public override bool IsValid(OpenPlayer source)
		{
			return true;
		}

		/// <inheritdoc />
		public override bool Execute(OpenPlayer source)
		{
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