using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAPI.Player;

namespace OpenAPI.Events.Block
{
    public class BlockAbortBreakEvent : BlockStartBreakEvent
    {
        public BlockAbortBreakEvent(OpenPlayer player, MiNET.Blocks.Block block) : base(player, block)
        {
        }
    }
}
