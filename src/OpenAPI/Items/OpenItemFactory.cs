using System;
using System.Collections.Concurrent;
using MiNET.Items;

namespace OpenAPI.Items
{
	public class OpenItemFactory : ICustomItemFactory
	{
		private ConcurrentDictionary<Tuple<short, short>, Func<Item>> RegisteredItems { get; }
		public OpenItemFactory()
		{
			RegisteredItems = new ConcurrentDictionary<Tuple<short, short>, Func<Item>>();
		}

		public Item GetItem(short id, short metadata, int count)
		{
			Func<Item> itemFactory;
			if (RegisteredItems.TryGetValue(new Tuple<short, short>(id, metadata), out itemFactory))
			{
				var item = itemFactory();
				item.Metadata = metadata;
				item.Count = (byte) count;

				return item;
			}
			return null;
		}

		public bool TryRegisterItem(short id, short metadata, Func<Item> itemFactory)
		{
			return RegisteredItems.TryAdd(new Tuple<short, short>(id, metadata), itemFactory);
		}
	}
}
