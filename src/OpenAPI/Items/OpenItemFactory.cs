using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using MiNET.Items;

namespace OpenAPI.Items
{
	public class OpenItemFactory : ICustomItemFactory
	{
		public record ItemKey(string Name, short Metadata);
		private ConcurrentDictionary<ItemKey, Func<Item>> RegisteredItems { get; }
		public OpenItemFactory()
		{
			RegisteredItems = new ConcurrentDictionary<ItemKey, Func<Item>>();
		}

		/// <summary>
		///		Items are keyed by registry name since 1.26 rather than by numeric id. MiNET resolves
		///		a bare name to the minecraft namespace and compares names case insensitively before
		///		it ever asks us, so keys are normalized the same way or a registration made without
		///		a namespace would simply never be found.
		/// </summary>
		private static ItemKey KeyOf(string name, short metadata)
		{
			if (string.IsNullOrEmpty(name))
				return new ItemKey(string.Empty, metadata);

			if (name.IndexOf(':') < 0)
				name = "minecraft:" + name;

			return new ItemKey(name.ToLowerInvariant(), metadata);
		}

		public Item GetItem(string id, short metadata, int count)
		{
			Func<Item> itemFactory;
			if (RegisteredItems.TryGetValue(KeyOf(id, metadata), out itemFactory))
			{
				var item = itemFactory();
				item.Metadata = metadata;
				item.Count = (byte) count;

				return item;
			}
			return null;
		}

		public bool TryRegisterItem(string id, short metadata, Func<Item> itemFactory)
		{
			return RegisteredItems.TryAdd(KeyOf(id, metadata), itemFactory);
		}

		/// <summary>
		///		Removes a previously registered item factory.
		/// </summary>
		/// <returns>Whether an item was registered for that id and metadata.</returns>
		public bool TryUnregisterItem(string id, short metadata)
		{
			return RegisteredItems.TryRemove(KeyOf(id, metadata), out _);
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
