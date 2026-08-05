using System;
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

	    public bool HasItems(string itemName, short meta, int count)
		{
			int c = 0;
			for (byte i = 0; i < Slots.Count; i++)
			{
				var slot = (Slots[i]);
				if (IsItem(slot, itemName) && slot.Metadata == meta)
				{
					c += slot.Count;
					if (c >= count) return true;
				}
			}
			return false;
		}

		/// <summary>
		///		Matches a slot against a registry name, tolerating a missing namespace and casing the
		///		same way MiNET's own name resolution does.
		/// </summary>
		private static bool IsItem(Item slot, string itemName)
		{
			if (slot == null || string.IsNullOrEmpty(itemName))
				return false;

			if (itemName.IndexOf(':') < 0)
				itemName = "minecraft:" + itemName;

			return string.Equals(slot.Name, itemName, StringComparison.OrdinalIgnoreCase);
		}

		public void TakeItems(string itemName, short meta, int count)
		{
			int remaining = count;
			for (byte i = 0; i < Slots.Count; i++)
			{
				var slot = (Slots[i]);
				if (IsItem(slot, itemName) && slot.Metadata == meta)
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
