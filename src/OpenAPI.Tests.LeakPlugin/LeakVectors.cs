using System;
using System.Threading;
using MiNET.Items;
using MiNET.Plugins.Attributes;
using OpenAPI.Commands;
using OpenAPI.Events;
using OpenAPI.Player;
using OpenAPI.Plugins;
using OpenAPI.World;

namespace OpenAPI.Tests.LeakPlugin
{
	/// <summary>
	///		A plugin-defined <see cref="Event"/>. Registering this type lands it in the
	///		dispatcher's event-type list, which pins this assembly.
	/// </summary>
	public class LeakTestEvent : Event
	{
	}

	/// <summary>
	///		A plugin-defined permission attribute, used as a dictionary *key* in
	///		CommandManager._permissionCheckers.
	/// </summary>
	public class LeakPermissionAttribute : CommandPermissionAttribute
	{
	}

	public class LeakPermissionChecker : CommandPermissionChecker<LeakPermissionAttribute>
	{
		public override bool HasPermission(LeakPermissionAttribute attr, OpenPlayer player) => true;
	}

	[OpenPluginInfo(Name = "Leak Test Plugin", Author = "OpenAPI tests", Version = "1.0")]
	public class LeakTestPlugin : OpenPlugin, IEventHandler
	{
		public override void Enabled(OpenApi api)
		{
		}

		public override void Disabled(OpenApi api)
		{
			// Deliberately does NOT unregister anything. Teardown must be driven by the
			// host, not by plugin author discipline — that is the whole point of the test.
		}

		[EventHandler]
		public void OnLeakTestEvent(LeakTestEvent e)
		{
		}
	}

	/// <summary>
	///		Instance method used as a <see cref="Func{Item}"/>, so both the delegate's
	///		target and its method live in this assembly.
	/// </summary>
	public class LeakItemFactory
	{
		public Item Create() => null;
	}

	/// <summary>
	///		Scheduled onto a level's <see cref="Utils.TickScheduler"/>, which outlives reloads.
	/// </summary>
	public class LeakScheduledWork
	{
		public void Tick()
		{
		}
	}

	public class LeakCommands
	{
		[Command(Name = "leaktest", Description = "Command registered by the leak test plugin")]
		public string LeakTest() => "leak";
	}

	/// <summary>
	///		Entry point invoked reflectively by the leak tests. Each case wires this
	///		assembly into the host through one public API, so the test can prove that
	///		host-driven teardown releases it again.
	/// </summary>
	public static class LeakVectors
	{
		public static object Apply(string vector, OpenApi api, OpenLevel level)
		{
			switch (vector)
			{
				// Registered on the LEVEL's dispatcher, not the root one. Levels outlive plugin
				// reloads, so a plugin unregistering from api.EventDispatcher still leaks these.
				case "levelevents":
				{
					var plugin = new LeakTestPlugin();
					level.EventDispatcher.RegisterEvents(plugin);
					return plugin;
				}

				// Repeating tick with CancellationToken.None: no cancellation path exists, so
				// only an assembly-scoped purge can ever remove it.
				case "tickscheduler":
				{
					var work = new LeakScheduledWork();
					level.TickScheduler.ScheduleTick(20, work.Tick, CancellationToken.None, repeat: true);
					return work;
				}

				// Control: loaded and instantiated, but never handed to the host.
				// Must always pass — if it fails, the harness itself is leaking.
				case "none":
					return new LeakTestPlugin();

				case "events":
				{
					var plugin = new LeakTestPlugin();
					api.EventDispatcher.RegisterEvents(plugin);
					return plugin;
				}

				case "eventtypes":
					api.EventDispatcher.RegisterEventType(typeof(LeakTestEvent));
					return new LeakTestPlugin();

				case "permissioncheckers":
					api.CommandManager.RegisterPermissionChecker(
						typeof(LeakPermissionAttribute), new LeakPermissionChecker());
					return new LeakTestPlugin();

				case "itemfactory":
				{
					var factory = new LeakItemFactory();
					api.ItemFactory.TryRegisterItem(31000, 0, factory.Create);
					return factory;
				}

				case "di":
				{
					var plugin = new LeakTestPlugin();
					api.PluginManager.Services.RegisterSingleton(plugin.GetType(), plugin);
					return plugin;
				}

				case "commands":
				{
					var commands = new LeakCommands();
					api.CommandManager.LoadCommands(commands);
					return commands;
				}

				default:
					throw new ArgumentException($"Unknown leak vector \"{vector}\"", nameof(vector));
			}
		}
	}
}
