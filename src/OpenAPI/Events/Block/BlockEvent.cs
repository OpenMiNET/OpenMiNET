namespace OpenAPI.Events.Block
{
	public class BlockEvent : Event
	{
		public MiNET.Blocks.Block Block { get; }
		public BlockEvent(MiNET.Blocks.Block block)
		{
			Block = block;
		}
	}
}
