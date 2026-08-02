using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
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

		/// <summary>
		///		Removes a previously registered item factory.
		/// </summary>
		/// <returns>Whether an item was registered for that id and metadata.</returns>
		public bool TryUnregisterItem(short id, short metadata)
		{
			return RegisteredItems.TryRemove(new Tuple<short, short>(id, metadata), out _);
		}

		/// <summary>
		///		Removes every item factory belonging to <paramref name="assembly"/>.
		/// </summary>
		/// <remarks>
		///		This factory is reachable from a MiNET static (ItemFactory.CustomItemFactory),
		///		so it is rooted for the lifetime of the process — a registered delegate keeps
		///		the plugin assembly alive forever unless it is removed here.
		///
		///		Both the delegate's target and its declaring type are checked: a factory may be
		///		a plugin instance method, a plugin static method, or a lambda, and they pin the
		///		assembly through different references.
		/// </remarks>
		public int PurgeAssembly(Assembly assembly)
		{
			int removed = 0;

			foreach (var registered in RegisteredItems.ToArray())
			{
				var factory = registered.Value;

				bool belongsToAssembly =
					factory.Target?.GetType().Assembly == assembly
					|| factory.Method?.DeclaringType?.Assembly == assembly;

				if (belongsToAssembly && RegisteredItems.TryRemove(registered.Key, out _))
					removed++;
			}

			return removed;
		}
	}
}
