using System;
using OpenAPI.Player;

namespace OpenAPI.Events.Block
{
	/// <summary>
	///		Gets dispatched when a <see cref="OpenPlayer"/> breaks a block
	/// </summary>
	public class BlockBreakEvent : BlockExpEvent
	{
		/// <summary>
		/// 	The player that broke the block
		/// </summary>
		public OpenPlayer Player { get; }
		
		/// <summary>
		/// 	
		/// </summary>
		/// <param name="player">The player that triggered the event</param>
		/// <param name="block">The block that was broken</param>
		public BlockBreakEvent(OpenPlayer player, MiNET.Blocks.Block block) : base(block, block.GetExperiencePoints())
		{
			Player = player;
		}
	}
}
