using OpenAPI.Player;

namespace OpenAPI.Events.Block
{
    public class BlockStartBreakEvent : BlockBreakEvent
    {
        public BlockStartBreakEvent(OpenPlayer player, MiNET.Blocks.Block block) : base(player, block)
        {

        }
    }
}
