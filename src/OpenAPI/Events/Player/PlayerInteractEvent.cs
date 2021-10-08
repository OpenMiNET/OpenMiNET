using MiNET;
using MiNET.Items;
using MiNET.Utils;
using MiNET.Utils.Vectors;
using OpenAPI.Player;

namespace OpenAPI.Events.Player
{
    /// <summary>
    ///     Dispatched whenever an <see cref="OpenPlayer"/> interacts with a block or air
    /// </summary>
    public class PlayerInteractEvent : PlayerEvent
    {
        /// <summary>
        ///     The type of interaction
        /// </summary>
        public PlayerInteractType InteractType { get; }
        
        /// <summary>
        ///     The item the player was holding
        /// </summary>
        public Item Item { get; }
        
        /// <summary>
        ///     The coordinates of the block the player interacted with
        /// </summary>
        public BlockCoordinates Coordinates { get; }
        
        /// <summary>
        ///     The face that the player hit
        /// </summary>
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
            /// <summary>
            ///     Player left clicked a block
            /// </summary>
            LeftClickBlock,
            
            /// <summary>
            ///     Player right clicked a block
            /// </summary>
            RightClickBlock,
            
            /// <summary>
            ///     Player left clicked in air
            /// </summary>
            LeftClickAir,
            
            /// <summary>
            ///     Player right clicked in air
            /// </summary>
            RightClickAir,
            
            /// <summary>
            /// Not used.
            /// </summary>
            Physical
        }
    }
}