using MiNET;
using MiNET.Items;
using MiNET.Utils;
using OpenAPI.Player;

namespace OpenAPI.Events.Player
{
    public class PlayerInteractEvent : PlayerEvent
    {
        public PlayerInteractType InteractType { get; }
        public Item Item { get; }
        public BlockCoordinates Coordinates { get; }
        public BlockFace Face { get; }
        
        public PlayerInteractEvent(OpenPlayer player, Item item, BlockCoordinates blockCoordinates, BlockFace face) :
            this(player, item, blockCoordinates, face, PlayerInteractType.RightClickBlock)
        {

        }

        public PlayerInteractEvent(OpenPlayer player, Item item, BlockCoordinates coordinates, BlockFace face,
            PlayerInteractType type) : base(player)
        {
            Item = item;
            Coordinates = coordinates;
            Face = face;
            InteractType = type;
        }

        public enum PlayerInteractType
        {
            LeftClickBlock,
            RightClickBlock,
            LeftClickAir,
            RightClickAir,
            
            /// <summary>
            /// Not used.
            /// </summary>
            Physical
        }
    }
}