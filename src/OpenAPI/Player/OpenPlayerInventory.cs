using log4net;
using MiNET;
using MiNET.Items;

namespace OpenAPI.Player
{
	public class OpenPlayerInventory : PlayerInventory
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(OpenPlayerInventory));
		
		public OpenPlayerInventory(OpenPlayer player) : base(player)
		{
			
		}

		public override Item GetItemInHand()
		{
		//	if (CursorItem != null && CursorItem.Id > 0)
		//	{
		//		return CursorItem;
		//	}

			var index = InHandSlot;
			if (index == -1 || index >= Slots.Count) return new ItemAir();

			return Slots[index] ?? new ItemAir();
		}

	  /*  public override bool HasItem(Item item)
	    {
	        return HasItems(item.Id, item.Metadata, item.Count);
	    }*/

	    public bool HasItems(short itemId, short meta, int count)
		{
			int c = 0;
			for (byte i = 0; i < Slots.Count; i++)
			{
				var slot = (Slots[i]);
				if (slot.Id == itemId && slot.Metadata == meta)
				{
					c += slot.Count;
					if (c >= count) return true;
				}
			}
			return false;
		}

		public void TakeItems(short itemId, short meta, int count)
		{
			int remaining = count;
			for (byte i = 0; i < Slots.Count; i++)
			{
				var slot = (Slots[i]);
				if (slot.Id == itemId && slot.Metadata == meta)
				{
					if (slot.Count > remaining)
					{
						slot.Count = (byte) (slot.Count - count);
						SetInventorySlot(i, slot);
						return;
					}
					else if (slot.Count == remaining)
					{
						SetInventorySlot(i, new ItemAir());
						return;
					}
					else if (slot.Count < remaining)
					{
						remaining -= slot.Count;
						SetInventorySlot(i, new ItemAir());
					}
				}
			}
		}
	}
}
