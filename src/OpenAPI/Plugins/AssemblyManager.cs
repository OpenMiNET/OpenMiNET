using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;

namespace OpenAPI.Plugins
{
    internal class AssemblyManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AssemblyManager));

        /// <summary>
        /// 	Assemblies that were already present when the manager was created — the host and
        /// 	everything it references.
        /// </summary>
        /// <remarks>
        /// 	Snapshotted once, deliberately. These must always resolve from the default context
        /// 	so plugins share the host's type identity; a plugin loading its own copy of
        /// 	OpenAPI would produce an <c>OpenPlugin</c> the host does not recognise.
        /// </remarks>
        private ConcurrentDictionary<string, Assembly> HostAssemblies { get; }

        /// <summary>
        /// 	Plugin assemblies, each in its own collectible context.
        /// </summary>
        private ConcurrentDictionary<string, Assembly> PluginAssemblies { get; }

        private ConcurrentDictionary<Assembly, PluginLoadContext> Contexts { get; }

        public AssemblyManager()
        {
            HostAssemblies = new ConcurrentDictionary<string, Assembly>();
            PluginAssemblies = new ConcurrentDictionary<string, Assembly>();
            Contexts = new ConcurrentDictionary<Assembly, PluginLoadContext>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                HostAssemblies.TryAdd(assembly.GetName().Name, assembly);
            }
        }

        internal bool TryGetHostAssembly(string assemblyName, out Assembly assembly)
        {
            return HostAssemblies.TryGetValue(assemblyName, out assembly);
        }

        internal bool TryGetPluginAssembly(string assemblyName, out Assembly assembly)
        {
            return PluginAssemblies.TryGetValue(assemblyName, out assembly);
        }

        /// <summary>
        /// 	Loads a plugin assembly into its own collectible context.
        /// </summary>
        public bool TryLoadAssemblyFromFile(string assemblyName, string file, out Assembly assembly)
        {
            try
            {
                var context = new PluginLoadContext($"plugin-{assemblyName}", file, this);

                assembly = context.LoadFromFile(file);

                if (PluginAssemblies.TryAdd(assemblyName, assembly))
                {
                    Contexts.TryAdd(assembly, context);
                    return true;
                }

                // Already loaded under this name — drop the context we just made rather than
                // leaking it.
                context.Unload();
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load assembly {assemblyName} even tho its path was found!", ex);
            }

            assembly = null;
            return false;
        }

        public bool TryGetAssembly(string assemblyName, out Assembly assembly)
        {
            return PluginAssemblies.TryGetValue(assemblyName, out assembly)
                   || HostAssemblies.TryGetValue(assemblyName, out assembly);
        }

        public bool IsLoaded(string assemblyName, out Assembly outAssembly)
        {
            return TryGetAssembly(assemblyName, out outAssembly);
        }

        /// <summary>
        /// 	Drops the loader's own references to <paramref name="assembly"/> and unloads its
        /// 	context.
        /// </summary>
        /// <remarks>
        /// 	Unloading is a request, not a guarantee: the context is only collected once every
        /// 	reference into it is gone, which is what
        /// 	<see cref="OpenPluginManager.PurgeAssembly"/> is responsible for. Leaving a stale
        /// 	entry here would both pin the assembly and make a reload silently return the old
        /// 	one through the <see cref="IsLoaded"/> short-circuit in <c>ProcessFile</c>.
        /// </remarks>
        public void Remove(Assembly assembly)
        {
            foreach (var reference in PluginAssemblies.ToArray())
            {
                if (reference.Value == assembly)
                    PluginAssemblies.TryRemove(reference.Key, out _);
            }

            if (Contexts.TryRemove(assembly, out var context))
            {
                context.Unload();
            }
        }
    }
}
