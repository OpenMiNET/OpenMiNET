using OpenAPI.Player;

namespace OpenAPI.Events.Block
{
	public class BlockPlaceEvent : BlockEvent
	{
		public OpenPlayer Player { get; }
		public BlockPlaceEvent(OpenPlayer player, MiNET.Blocks.Block block) : base(block)
		{
			Player = player;
		}
	}
}
