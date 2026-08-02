using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using OpenAPI.Plugins;
using OpenAPI.Utils;

namespace OpenAPI.Events
{
	/// <summary>
	/// 	The <see cref="EventDispatcher"/> is responsible for dispatching and invoking all the registered <see cref="IEventHandler"/> methods
	/// </summary>
	public class EventDispatcher : IAssemblyPurgeable
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(EventDispatcher));

		/// <summary>
		/// 	Registers a new <see cref="Event"/> type with the current EventDispatcher
		/// </summary>
		/// <typeparam name="TEvent">The type of the <see cref="Event"/> to register</typeparam>
		/// <exception cref="DuplicateTypeException">Thrown when the type of <typeparamref name="TEvent"/> has already been registered.</exception>
		public void RegisterEventType<TEvent>() where TEvent : Event
		{
			Type t = typeof(TEvent);
			if (!RegisterEventType(t))
			{
				throw new DuplicateTypeException();
			}
		}

		/// <summary>
		/// Registers a new <see cref="Event"/>
		/// </summary>
		/// <param name="type">The type of the <see cref="Event"/> to register</param>
		/// <returns>Whether the event was succesfully registered.</returns>
		public bool RegisterEventType(Type type)
		{
			if (!RegisteredEvents.TryAdd(type, new EventDispatcherValues()))
			{
				return false;
			}

			Log.Info($"Registered event type \"{type.Name}\"");
			return true;
		}

		/// <summary>
		/// 	The <see cref="Event"/> types currently known to this dispatcher.
		/// </summary>
		internal Type[] KnownEventTypes => RegisteredEvents.Keys.ToArray();
		
		/// <summary>
		/// 	Loads all types implementing the <see cref="Event"/> class
		/// </summary>
		/// <param name="assembly">The assembly containing the <see cref="Event"/> implementations</param>
		public void LoadFrom(Assembly assembly)
		{
			var count = GetEventTypes(assembly).Count(RegisterEventType);
			Log.Info($"Registered {count} event types from assembly {assembly.ToString()}");
		}

		/// <summary>
		/// 	Releases every reference this dispatcher holds into <paramref name="assembly"/>:
		/// 	the <see cref="Event"/> types it declares, and every handler registered by an
		/// 	object belonging to it.
		/// </summary>
		/// <remarks>
		/// 	Each registered handler pins the assembly twice over — once through the handler
		/// 	instance and once through the <see cref="MethodInfo"/> — so both are matched.
		///
		/// 	This must not rely on plugins calling <see cref="UnregisterEvents"/> in
		/// 	<see cref="Plugins.OpenPlugin.Disabled"/>; several do not.
		/// </remarks>
		/// <param name="assembly">The assembly to release.</param>
		public int PurgeAssembly(Assembly assembly)
		{
			int handlers = 0;
			int types = 0;

			foreach (var registered in RegisteredEvents.ToArray())
			{
				handlers += registered.Value.RemoveAssembly(assembly);

				// The key Type itself pins the assembly, so the whole entry has to go.
				if (registered.Key.Assembly == assembly && RegisteredEvents.TryRemove(registered.Key, out _))
					types++;
			}

			if (handlers > 0 || types > 0)
				Log.Info($"Purged {handlers} event handlers and {types} event types from assembly {assembly.GetName().Name}");

			return handlers + types;
		}
		
		private static IEnumerable<Type> GetEventTypes(Assembly assembly)
		{
			return assembly.GetTypes().Where(p =>
			{
				if (p.IsClass && !p.IsAbstract && typeof(Event).IsAssignableFrom(p))
				{
					return true;
				}

				return false;
			});
		}

		/// <remarks>
		/// 	Concurrent rather than a plain <see cref="Dictionary{TKey,TValue}"/> because
		/// 	registration and assembly purges happen on a live server while events are being
		/// 	dispatched from the tick loop.
		/// </remarks>
		private ConcurrentDictionary<Type, EventDispatcherValues> RegisteredEvents { get; }
		protected OpenApi Api { get; }
		private EventDispatcher[] ExtraDispatchers { get; }

		public EventDispatcher(OpenApi openApi, params EventDispatcher[] dispatchers)
		{
			Api = openApi;
			ExtraDispatchers = dispatchers.Where(x => x != this).ToArray();

			RegisteredEvents = new ConcurrentDictionary<Type, EventDispatcherValues>();

			// A chained dispatcher (one per level) inherits what its parent already knows;
			// only the root has to scan. Previously this came from a static list shared by
			// every dispatcher, which meant a plugin's event types could never be released.
			IEnumerable<Type> seed = ExtraDispatchers.Length > 0
				? ExtraDispatchers.SelectMany(x => x.KnownEventTypes).Distinct()
				: AppDomain.CurrentDomain.GetAssemblies().SelectMany(GetEventTypes);

			foreach (var eventType in seed)
			{
				RegisteredEvents.TryAdd(eventType, new EventDispatcherValues());
			}

			Api?.RegisterPurgeable(this);
		}

		/// <summary>
		/// 	Registers all EventHandler methods with the current EventDispatcher.
		/// </summary>
		/// <param name="obj">The class to scan for EventHandlers</param>
		public void RegisterEvents(IEventHandler obj)
		{
			int count = 0;

			var type = typeof(Event);
			Type objType = obj.GetType();
			foreach (var method in objType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
			{
				EventHandlerAttribute attribute = method.GetCustomAttribute<EventHandlerAttribute>(false);
				if (attribute == null) continue;

				var parameters = method.GetParameters();
				if (parameters.Length != 1 || !type.IsAssignableFrom(parameters[0].ParameterType)) continue;

				var paramType = parameters[0].ParameterType;

				// GetOrAdd rather than the previous ContainsKey/TryAdd pair: when the type was
				// already known globally but absent from this dispatcher, that left 'e' null
				// and threw a NullReferenceException on the next line.
				EventDispatcherValues e = RegisteredEvents.GetOrAdd(paramType, _ => new EventDispatcherValues());

				if (!e.RegisterEventHandler(attribute, obj, method))
				{
					Log.Warn($"Duplicate found for class \"{obj.GetType()}\" of type \"{paramType}\"");
				}
				else
				{
					count++;
				}
			}

			Log.Info($"Registered {count} event handlers for \"{obj}\"");
		}

		/// <summary>
		/// 	Unregisters all <see cref="EventHandlerAttribute"/> from the specified <see cref="IEventHandler"/> implementation from the current EventDispatcher
		/// 	After UnRegistering, the class will no longer get invoked when an event gets dispatched.
		/// </summary>
		/// <param name="obj">The implementation to unregister the eventhandlers for</param>
		public void UnregisterEvents(IEventHandler obj)
		{
			foreach (var kv in RegisteredEvents.ToArray())
			{
				kv.Value.Clear(obj);
			}
		}

		private async Task DispatchPrivate(Event e)
		{
			try
			{
				Type type = e.GetType();
				if (RegisteredEvents.TryGetValue(type, out EventDispatcherValues v))
				{
					await v.DispatchAsync(e);
				}
				else
				{
					Log.Warn($"Unknown event type found! \"{type}\"");
				}
			}
			catch (Exception ex)
			{
				Log.Error("Error while dispatching event!", ex);
			}
		}

		/// <summary>
		/// 	Dispatches a new <see cref="Event"/> to all methods registered with an <see cref="EventHandlerAttribute"/>
		/// </summary>
		/// <param name="e">The event to dispatch</param>
		public void DispatchEvent(Event e)
		{
			DispatchPrivate(e).Wait();

			if (!e.IsCancelled)
			{
				foreach (var i in ExtraDispatchers)
				{
					i.DispatchPrivate(e).Wait();
					if (e.IsCancelled) break;
				}
			}

			if (Api.ServerInfo != null)
			{
				Interlocked.Increment(ref Api.ServerInfo.EventsDispatchedPerSecond);
			}
		}

		/// <summary>
		/// 	Dispatches 
		/// </summary>
		/// <param name="e"></param>
		/// <typeparam name="TEvent"></typeparam>
		/// <returns></returns>
		public async Task<TEvent> DispatchEventAsync<TEvent>(TEvent e) where TEvent : Event
		{
			await DispatchPrivate(e);

			List<Task> dispatchTasks = new List<Task>();
			
			if (!e.IsCancelled)
			{
				foreach (var i in ExtraDispatchers)
				{
					dispatchTasks.Add(i.DispatchPrivate(e));
					//if (e.IsCancelled) break;
				}
			}

			await Task.WhenAll(dispatchTasks);

			if (Api.ServerInfo != null)
			{
				Interlocked.Increment(ref Api.ServerInfo.EventsDispatchedPerSecond);
			}

			return e;
		}

		private class EventDispatcherValues
		{
			/// <summary>
			/// 	Ascending, so handlers run lowest priority first and Monitor last.
			/// 	Held explicitly because <see cref="ConcurrentDictionary{TKey,TValue}"/>
			/// 	does not guarantee enumeration order.
			/// </summary>
			private static readonly EventPriority[] Priorities =
				Enum.GetValues(typeof(EventPriority)).Cast<EventPriority>().OrderBy(x => (int) x).ToArray();

			/// <remarks>
			/// 	Values are immutable snapshots, replaced wholesale on register/remove. A
			/// 	dispatch in flight keeps iterating the array it started with, so unloading a
			/// 	plugin cannot mutate a list out from under the tick loop.
			/// </remarks>
			private ConcurrentDictionary<EventPriority, Item[]> Items { get; }

			public EventDispatcherValues()
			{
				Items = new ConcurrentDictionary<EventPriority, Item[]>();
				foreach (var priority in Priorities)
				{
					Items.TryAdd(priority, Array.Empty<Item>());
				}
			}

			public bool RegisterEventHandler(EventHandlerAttribute attribute, IEventHandler parent, MethodInfo method)
			{
				var item = new Item(attribute, parent, method);

				Items.AddOrUpdate(
					attribute.Priority,
					_ => new[] { item },
					(_, existing) =>
					{
						var updated = new Item[existing.Length + 1];
						Array.Copy(existing, updated, existing.Length);
						updated[existing.Length] = item;
						return updated;
					});

				return true;
			}

			public void Clear(IEventHandler parent)
			{
				RemoveWhere(item => ReferenceEquals(item.Parent, parent));
			}

			/// <summary>
			/// 	Drops every handler belonging to <paramref name="assembly"/> and reports how
			/// 	many were removed.
			/// </summary>
			public int RemoveAssembly(Assembly assembly)
			{
				// Both the handler instance and the MethodInfo reference the plugin assembly,
				// and they are not always the same one: a plugin can register a handler whose
				// method is declared on a host base class, or vice versa. Either pins it.
				return RemoveWhere(item =>
					item.Parent?.GetType().Assembly == assembly
					|| item.Method?.DeclaringType?.Assembly == assembly);
			}

			private int RemoveWhere(Func<Item, bool> predicate)
			{
				int removed = 0;

				foreach (var priority in Priorities)
				{
					// Compare-and-swap rather than AddOrUpdate: its update factory may run more
					// than once under contention, which would double-count the removals.
					while (Items.TryGetValue(priority, out var existing))
					{
						var kept = existing.Where(x => !predicate(x)).ToArray();

						if (kept.Length == existing.Length)
							break;

						if (Items.TryUpdate(priority, kept, existing))
						{
							removed += existing.Length - kept.Length;
							break;
						}
					}
				}

				return removed;
			}

			/*public void Dispatch(Event e)
			{
				object[] args = {
					e
				};

			    foreach (var priority in Items)
			    {
			        Parallel.ForEach(priority.Value.ToArray(), pair =>
			        {
			            if (e.IsCancelled &&
			                pair.Attribute.IgnoreCanceled)
			                return;

			            pair.Method.Invoke(pair.Parent, args);
			        });
                }
			}*/

			public async Task DispatchAsync(Event e)
			{
				object[] args = {
					e
				};

				foreach (var priority in Priorities)
				{
					// Snapshot: a concurrent register or plugin unload swaps the array, it does
					// not mutate the one we are iterating.
					if (!Items.TryGetValue(priority, out var handlers))
						continue;

					Task[] tasks = new Task[handlers.Length];
					for (var index = 0; index < handlers.Length; index++)
					{
						var p = handlers[index];

						var method = p.Method;
						if (method.ReturnType == typeof(void))
						{
							tasks[index] = Task.Run(() =>
							{
								if (e.IsCancelled &&
								    p.Attribute.IgnoreCanceled)
									return;
								
								method.Invoke(p.Parent, args);
							});
						}
						else if (typeof(Task).IsAssignableFrom(method.ReturnType))
						{
							tasks[index] = Task.Run(async () =>
							{
								if (e.IsCancelled &&
								    p.Attribute.IgnoreCanceled)
									return;
								
								await (Task) method.Invoke(p.Parent, args);
							});
						}
					}

					await Task.WhenAll(tasks);
				}
			}

			private struct Item : IComparable<Item>
			{
				//public EventPriority Priority;
				public EventHandlerAttribute Attribute;
				public IEventHandler Parent;
				public MethodInfo Method;
				public Item(EventHandlerAttribute attribute, IEventHandler parent, MethodInfo method)
				{
					Attribute = attribute;
					Parent = parent;
					Method = method;
				}

				public int CompareTo(Item other)
				{
					int result = Attribute.Priority.CompareTo(other.Attribute.Priority);

					if (result == 0)
						result = Parent.GetHashCode().CompareTo(other.Parent.GetHashCode());
					
						return result;
				}
			}

			//private class ItemCompare
		}
	}
}
