using System;
using OpenAPI.Player;

namespace OpenAPI.Events.Block
{
	public class BlockBreakEvent : BlockExpEvent
	{
		public OpenPlayer Player { get; }
		public BlockBreakEvent(OpenPlayer player, MiNET.Blocks.Block block) : base(block, block.GetExperiencePoints())
		{
			Player = player;
		}
	}
}
