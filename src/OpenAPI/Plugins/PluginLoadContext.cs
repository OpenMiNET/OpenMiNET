using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using log4net;

namespace OpenAPI.Plugins
{
	/// <summary>
	///		A collectible load context holding a single plugin and its private dependencies.
	/// </summary>
	/// <remarks>
	///		Two things this buys over loading everything into the default context:
	///		plugins no longer clobber each other's transitive dependencies (previously the first
	///		plugin to load a given library won for every plugin), and the assembly can actually
	///		be unloaded once the host has released its references.
	///
	///		Note that isolation here is about assembly identity and unloadability only. It is
	///		<em>not</em> a security or fault boundary — a plugin that throws, blocks, or calls
	///		Environment.Exit still takes the server with it.
	/// </remarks>
	internal sealed class PluginLoadContext : AssemblyLoadContext
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(PluginLoadContext));

		private readonly AssemblyDependencyResolver _resolver;
		private readonly AssemblyManager _assemblyManager;

		internal PluginLoadContext(string name, string pluginPath, AssemblyManager assemblyManager)
			: base(name, isCollectible: true)
		{
			_resolver = new AssemblyDependencyResolver(pluginPath);
			_assemblyManager = assemblyManager;
		}

		/// <summary>
		///		Reads an assembly without keeping the file open.
		/// </summary>
		/// <remarks>
		///		Deliberately not <see cref="AssemblyLoadContext.LoadFromAssemblyPath"/>: that
		///		memory-maps the file and holds it open, which would stop a new build of the
		///		plugin being dropped in while the server is running — the whole point of reload.
		/// </remarks>
		internal Assembly LoadFromFile(string path)
		{
			byte[] assemblyBytes = File.ReadAllBytes(path);

			// Symbols are loaded separately so stack traces survive a reload, and are optional.
			string symbolsPath = Path.ChangeExtension(path, ".pdb");

			using (var assemblyStream = new MemoryStream(assemblyBytes))
			{
				if (File.Exists(symbolsPath))
				{
					try
					{
						using (var symbolStream = new MemoryStream(File.ReadAllBytes(symbolsPath)))
						{
							return LoadFromStream(assemblyStream, symbolStream);
						}
					}
					catch (Exception ex)
					{
						Log.Debug($"Could not load symbols for \"{path}\", continuing without.", ex);
						assemblyStream.Position = 0;
					}
				}

				return LoadFromStream(assemblyStream);
			}
		}

		/// <inheritdoc />
		protected override Assembly Load(AssemblyName assemblyName)
		{
			// 1. Anything the host already has must be shared, never duplicated. OpenAPI and
			//    MiNET sit next to the plugin on disk, so without this the resolver below would
			//    happily load a second copy and split type identity — OpenPlugin from this
			//    context would not be the OpenPlugin the host is looking for, and nothing would
			//    resolve.
			if (_assemblyManager.TryGetHostAssembly(assemblyName.Name, out var hostAssembly))
				return hostAssembly;

			// 2. Another plugin's assembly resolves to *its* context, so cross-plugin
			//    constructor injection keeps working: the dependent must see the same type
			//    identity as the plugin it binds to.
			if (_assemblyManager.TryGetPluginAssembly(assemblyName.Name, out var pluginAssembly))
				return pluginAssembly;

			// 3. A private dependency shipped alongside this plugin, loaded into this context so
			//    two plugins can use different versions of the same library.
			string path = _resolver.ResolveAssemblyToPath(assemblyName);
			if (path != null && File.Exists(path))
				return LoadFromFile(path);

			// Fall through to the default context.
			return null;
		}

		/// <inheritdoc />
		protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
		{
			string path = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);

			return path != null ? LoadUnmanagedDllFromPath(path) : IntPtr.Zero;
		}
	}
}
