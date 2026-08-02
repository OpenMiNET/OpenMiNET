using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using MiNET.Worlds;
using OpenAPI.World;
using Xunit;
using Xunit.Abstractions;

// GC assertions are process-wide; concurrent tests make them flaky.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace OpenAPI.Tests
{
	/// <summary>
	///		Proves that a plugin assembly is fully released after the host tears it down.
	///
	///		A missed reference fails <em>silently</em> — the collectible
	///		<see cref="AssemblyLoadContext"/> simply never unloads, with no exception and no log
	///		line. This harness turns each known holder into a test that fails loudly instead.
	///
	///		Each case loads the leak plugin into its own collectible ALC, wires it into a real
	///		<see cref="OpenApi"/> host through one public API, asks the host to purge the
	///		assembly, and asserts the ALC is collected.
	/// </summary>
	public class PluginUnloadTests
	{
		private readonly ITestOutputHelper _output;

		public PluginUnloadTests(ITestOutputHelper output)
		{
			_output = output;
		}

		private static string LeakPluginPath =>
			Path.Combine(AppContext.BaseDirectory, "LeakPlugin", "OpenAPI.Tests.LeakPlugin.dll");

		[Fact]
		public void LeakPluginIsBuiltAndDiscoverable()
		{
			Assert.True(File.Exists(LeakPluginPath),
				$"Leak plugin not found at {LeakPluginPath}. The CopyLeakPlugin target should place it there.");
		}

		/// <summary>
		///		"none" is the control: the assembly is loaded and instantiated but never handed
		///		to the host, so it must always be collectable. If this fails, the harness itself
		///		is holding a reference and every other result is meaningless.
		/// </summary>
		[Theory]
		[InlineData("none")]
		[InlineData("commands")]
		[InlineData("di")]
		[InlineData("events")]
		[InlineData("eventtypes")]
		[InlineData("permissioncheckers")]
		[InlineData("itemfactory")]
		[InlineData("levelevents")]
		[InlineData("tickscheduler")]
		public void PluginAssemblyIsCollectedAfterPurge(string vector)
		{
			var api = new OpenApi();
			var level = CreateLevel(api);

			var alcRef = RunScenario(api, level, vector);

			bool collected = TryCollect(alcRef);

			// The level must outlive the plugin — that is the whole point of the level-scoped
			// vectors. Without this the level could be collected first and the test would pass
			// for the wrong reason.
			GC.KeepAlive(level);
			GC.KeepAlive(api);

			_output.WriteLine(collected
				? $"vector '{vector}': assembly released"
				: $"vector '{vector}': LEAK — assembly still reachable after purge");

			Assert.True(collected,
				$"Plugin assembly was not released after purge for vector '{vector}'. " +
				"Something in the host still holds an instance, Type, MethodInfo or delegate " +
				"belonging to the plugin assembly.");
		}

		/// <summary>
		///		A minimally constructed level. It is never initialised or ticked — the tests only
		///		need its <see cref="Events.EventDispatcher"/> and TickScheduler to exist and to
		///		have registered themselves with the host.
		/// </summary>
		private static OpenLevel CreateLevel(OpenApi api)
		{
			string directory = Path.Combine(Path.GetTempPath(), "openapi-leak-tests-" + Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(directory);

			var provider = new AnvilWorldProvider(directory)
			{
				MissingChunkProvider = new SuperflatGenerator(Dimension.Overworld)
			};

			return new OpenLevel(api, api.LevelManager, "leak-tests", provider, api.LevelManager.EntityManager);
		}

		/// <summary>
		///		Kept in its own non-inlined method so every local referencing the ALC is out of
		///		scope by the time the caller collects. In Debug builds locals stay rooted until
		///		the end of their enclosing method, so this cannot be folded into the caller.
		/// </summary>
		[MethodImpl(MethodImplOptions.NoInlining)]
		private static WeakReference RunScenario(OpenApi api, OpenLevel level, string vector)
		{
			var alc = new AssemblyLoadContext($"leak-test-{vector}", isCollectible: true);

			// LoadFromStream rather than LoadFromAssemblyPath: the path form memory-maps and
			// holds the file open, which would block replacing a plugin DLL on a live server.
			Assembly assembly;
			using (var stream = new MemoryStream(File.ReadAllBytes(LeakPluginPath)))
			{
				assembly = alc.LoadFromStream(stream);
			}

			var vectors = assembly.GetType("OpenAPI.Tests.LeakPlugin.LeakVectors", throwOnError: true);
			var apply = vectors.GetMethod("Apply", BindingFlags.Public | BindingFlags.Static);

			// Return value is deliberately discarded — holding it would pin the assembly.
			apply.Invoke(null, new object[] { vector, api, level });

			// The behaviour under test: host-driven, assembly-scoped teardown.
			api.PluginManager.PurgeAssembly(assembly);

			alc.Unload();
			return new WeakReference(alc);
		}

		/// <summary>
		///		The production path end to end: discover and enable a plugin through
		///		<see cref="Plugins.OpenPluginManager.DiscoverPlugins"/>, then unload it through
		///		<see cref="Plugins.OpenPluginManager.UnloadAll"/> and assert the assembly is
		///		actually released.
		/// </summary>
		/// <remarks>
		///		The other tests drive teardown directly, which proves the registries release
		///		their references but says nothing about whether the real loader does. This is the
		///		one that proves a reload would work.
		/// </remarks>
		[Fact]
		public void PluginIsReleasedAfterUnloadThroughTheLoader()
		{
			var api = new OpenApi();

			string pluginDirectory = CreatePluginDirectory();

			var assemblyRef = DiscoverEnableAndUnload(api, pluginDirectory);

			bool collected = TryCollect(assemblyRef);
			GC.KeepAlive(api);

			_output.WriteLine(collected
				? "loader round-trip: plugin assembly released"
				: "loader round-trip: LEAK — plugin assembly still reachable after UnloadAll");

			Assert.True(collected,
				"The plugin assembly was not released after UnloadAll. Either the loader is not " +
				"using a collectible load context, or something in the host still references it.");
		}

		/// <summary>
		///		Reload semantics: after unloading, discovering again must produce a genuinely new
		///		assembly rather than handing back the one already in memory.
		/// </summary>
		/// <remarks>
		///		The loader short-circuits <c>ProcessFile</c> on <c>AssemblyManager.IsLoaded</c>, so
		///		a stale bookkeeping entry would make a reload silently no-op — the server would
		///		report success and keep running the old code.
		/// </remarks>
		[Fact]
		public void ReloadingAPluginProducesAFreshAssembly()
		{
			var api = new OpenApi();

			string pluginDirectory = CreatePluginDirectory();

			Assembly first = DiscoverAndEnable(api, pluginDirectory);
			api.PluginManager.UnloadAll();

			Assembly second = DiscoverAndEnable(api, pluginDirectory);

			Assert.NotSame(first, second);
			Assert.Equal(first.GetName().Name, second.GetName().Name);

			_output.WriteLine($"reload produced a fresh assembly: {first.GetName().Name}");
		}

		/// <summary>
		///		Builds a plugin directory laid out the way the build actually produces one.
		/// </summary>
		/// <remarks>
		///		Crucially this copies OpenAPI.dll in alongside the plugin, because
		///		build/release/Plugins really does contain it. That is the case that breaks if the
		///		load context resolves host assemblies through AssemblyDependencyResolver instead
		///		of sharing the already-loaded one: the plugin would get a second copy of OpenAPI,
		///		its OpenPlugin would not be the host's OpenPlugin, and nothing would resolve.
		/// </remarks>
		private static string CreatePluginDirectory()
		{
			string directory = Path.Combine(Path.GetTempPath(), "openapi-reload-" + Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(directory);

			File.Copy(LeakPluginPath, Path.Combine(directory, Path.GetFileName(LeakPluginPath)));

			string hostAssembly = typeof(OpenApi).Assembly.Location;
			if (!string.IsNullOrEmpty(hostAssembly) && File.Exists(hostAssembly))
			{
				File.Copy(hostAssembly, Path.Combine(directory, Path.GetFileName(hostAssembly)), true);
			}

			return directory;
		}

		private static Assembly DiscoverAndEnable(OpenApi api, string pluginDirectory)
		{
			api.PluginManager.DiscoverPlugins(pluginDirectory);
			api.PluginManager.EnablePlugins();

			var loaded = api.PluginManager.GetLoadedPlugins();
			Assert.NotEmpty(loaded);

			return loaded[0].Instance.GetType().Assembly;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static WeakReference DiscoverEnableAndUnload(OpenApi api, string pluginDirectory)
		{
			api.PluginManager.DiscoverPlugins(pluginDirectory);
			api.PluginManager.EnablePlugins();

			var loaded = api.PluginManager.GetLoadedPlugins();
			Assert.NotEmpty(loaded);

			var assembly = loaded[0].Instance.GetType().Assembly;
			Assert.NotSame(typeof(PluginUnloadTests).Assembly, assembly);

			var reference = new WeakReference(assembly);

			api.PluginManager.UnloadAll();

			return reference;
		}

		private static bool TryCollect(WeakReference reference)
		{
			for (int i = 0; i < 15 && reference.IsAlive; i++)
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}

			return !reference.IsAlive;
		}
	}
}
