using MiNET.Items;

namespace OpenAPI.Player.Inventory
{
	public class CreateItemAction : InventoryAction
	{
		/// <inheritdoc />
		public CreateItemAction(Item sourceItem) : base(sourceItem, new ItemAir()) { }

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