using fNbt;
using MiNET.BlockEntities;
using OpenAPI.Player;

namespace OpenAPI.Events.Player
{
	/// <summary>
	/// 	Dispatched when an <see cref="OpenPlayer"/> tries to modify a blockentity's nbt data
	/// </summary>
	public class PlayerSetBlockEntityDataEvent : PlayerEvent
	{
		public BlockEntity BlockEntity { get; }
		public NbtCompound Compound { get; }
		
		/// <inheritdoc />
		public PlayerSetBlockEntityDataEvent(OpenPlayer player, BlockEntity blockEntity, NbtCompound compound) : base(player)
		{
			BlockEntity = blockEntity;
			Compound = compound;
		}
	}
}