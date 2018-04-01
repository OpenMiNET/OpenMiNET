using MiNET.Net;
using OpenAPI.Player;

namespace OpenAPI.Events.Entity
{
	public class EntityInteractEvent : EntityEvent
	{
		public OpenPlayer SourcePlayer { get; }
		public McpeInventoryTransaction.ItemUseOnEntityAction Action { get; }
		public EntityInteractEvent(MiNET.Entities.Entity entity, OpenPlayer source, McpeInventoryTransaction.ItemUseOnEntityAction action) : base(entity)
		{
			SourcePlayer = source;
			Action = action;
		}
	}
}
