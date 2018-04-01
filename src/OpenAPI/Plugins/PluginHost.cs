using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using log4net;

namespace OpenAPI.Plugins
{
	public delegate void PluginEventHandler(string message);
	public class PluginHost : MarshalByRefObject
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(PluginHost));
		
		private AppDomain Domain { get; set; }
		private OpenPluginManager PluginManager { get; set; }
		private List<Assembly> LoadedAssemblies { get; set; } = new List<Assembly>();
		private object _assemblyLock = new object();
		public PluginHost() { }

		public Assembly LoadAssembly(string path)
		{
			lock (_assemblyLock)
			{
				Assembly pluginAssembly = Assembly.LoadFile(path);

				if (!LoadedAssemblies.Contains(pluginAssembly))
					LoadedAssemblies.Add(pluginAssembly);

				return pluginAssembly;
			}
		}

		public bool IsLoaded(AssemblyName assembly, out Assembly outAssembly)
		{
			Assembly[] loadedAssemblies = Domain.GetAssemblies();
			Assembly ooutAssembly =
				loadedAssemblies.FirstOrDefault(x => x.GetName().Name
					.Equals(assembly.Name, StringComparison.InvariantCultureIgnoreCase));

			if (ooutAssembly != null)
			{
				outAssembly = ooutAssembly;
				return true;
			}
			outAssembly = null;
			return false;
		}

		public void Init(AppDomain domain, OpenPluginManager pluginManager)
		{
			Domain = domain;
			PluginManager = pluginManager;
			//domain.AssemblyResolve += TryResolveAssembly;
		}

		public bool TryLoad(string path, IEnumerable<AssemblyName> assemblyNames, out Assembly[] assemblies)
		{
			Dictionary<AssemblyName, Assembly> resolvedAssemblies = new Dictionary<AssemblyName, Assembly>();
			Dictionary<AssemblyName, string> resolvedPaths = new Dictionary<AssemblyName, string>();
			foreach (var assemblyName in assemblyNames)
			{
				if (IsLoaded(assemblyName, out Assembly loadedAssembly))
				{
					resolvedAssemblies.Add(assemblyName, loadedAssembly);
					continue;
				}

				try
				{
					if (PluginManager.TryFindAssemblyPath(assemblyName, path, out string resultPath))
					{
						resolvedPaths.Add(assemblyName, resultPath);
					}
					else
					{
						Log.Warn($"Could not find path for {assemblyName}");
						assemblies = default(Assembly[]);
						return false;
					}
				}
				catch
				{
					assemblies = default(Assembly[]);
					return false;
				}
			}

			foreach (var resolved in resolvedPaths)
			{
				try
				{
					Assembly assembly = LoadAssembly(resolved.Value);

					resolvedAssemblies.Add(resolved.Key, assembly);
					//AssemblyReferences.TryAdd(resolved.Key.Name, assembly);
				}
				catch (Exception ex)
				{
					Log.Error($"Failed to load assembly {resolved.Key} even tho its path was found!", ex);

					assemblies = default(Assembly[]);
					return false;
				}
			}

			assemblies = resolvedAssemblies.Values.ToArray();
			return true;
		}

		/*private Assembly TryResolveAssembly(object sender, ResolveEventArgs args)
		{
			if (PluginManager.TryResolve(this, Domain.BaseDirectory, new[] {new AssemblyName(args.Name)},
				out Assembly[] loadedAssemblies))
			{
				
			}
		}
		*/
		public Assembly[] GetPluginAssemblies()
		{
			return LoadedAssemblies.ToArray();
		}
	}
}
