namespace OpenAPI.Events.Block
{
	public class BlockExpEvent : BlockEvent
	{
		public float Experience { get; set; }
		public BlockExpEvent(MiNET.Blocks.Block block, float experience) : base(block)
		{
			Experience = experience;
		}
	}
}
