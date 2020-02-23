using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAPI.Player;

namespace OpenAPI.Events.Block
{
    /// <summary>
    ///     Get's dispatched whenever a <see cref="OpenPlayer"/> stops breaking a block before the block could be destroyed.
    /// </summary>
    public class BlockAbortBreakEvent : BlockStartBreakEvent
    {
        /// <summary>
        ///     
        /// </summary>
        /// <param name="player">The player that triggered the event</param>
        /// <param name="block">The block that the player was targetting</param>
        public BlockAbortBreakEvent(OpenPlayer player, MiNET.Blocks.Block block) : base(player, block)
        {
        }
    }
}
