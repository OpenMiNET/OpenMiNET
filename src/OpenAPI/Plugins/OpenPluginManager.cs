using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using MiNET.Utils;
using Mono.Cecil;
using OpenAPI.Events.Plugins;

namespace OpenAPI.Plugins
{
    public sealed class OpenPluginManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(OpenPluginManager));
        
        private Dictionary<Assembly, LoadedAssembly> LoadedAssemblies { get; }
		
		
		private OpenApi Parent { get; }
		private Assembly HostAssembly { get; }
		private AssemblyManager AssemblyManager { get; }
		private AssemblyResolver AssemblyResolver { get; }
		
		/// <summary>
		/// 	The dependency injection service container used when loading plugins.
		/// </summary>
		public DependencyContainer Services { get; }
        internal OpenPluginManager(OpenApi parent)
        {
            Parent = parent;
			HostAssembly = Assembly.GetAssembly(typeof(OpenPluginManager));
			
            LoadedAssemblies = new Dictionary<Assembly, LoadedAssembly>();
			//References = new ConcurrentDictionary<Type, object>();
			
			AssemblyManager = new AssemblyManager();
			AssemblyResolver = new AssemblyResolver(AssemblyManager);
			
			Services = new DependencyContainer();
			Services.RegisterSingleton<OpenApi>(parent);
        }

        /// <summary>
        /// 	Scans the specified paths for plugins & loads them into the AppDomain
        /// </summary>
        /// <param name="paths"></param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public void DiscoverPlugins(params string[] paths)
        {
	        int pluginInstances = 0;
	        
	        paths = paths.Where(x =>
	        {
		        if (Directory.Exists(x))
		        {
			        return true;
		        }
		        else
		        {
			        Log.Warn($"Could not load plugins from folder \"{x}\", folder does not exist!");
			        return false;
		        }
	        }).ToArray();
	        
	        foreach (var path in paths)
	        {
		        if (!Directory.Exists(path))
			        throw new DirectoryNotFoundException("Directory not found: " + path);
	        }

	        Dictionary<Assembly, string> loadedAssemblies = new Dictionary<Assembly, string>();
	        //List<(Assembly assembly, string path)> loadedAssemblies = new List<(Assembly assembly, string path)>();
	        int processed = 0;

	        foreach (var rawPath in paths)
	        {
		        string path = rawPath;

		        string[] files = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);
		        foreach (string file in files)
		        {
			        try
			        {
				        string filename = Path.GetFileNameWithoutExtension(file);
				        if (!Config.GetProperty($"plugin.{filename}.enabled", true))
				        {
					        Log.Info($"Not loading \"{Path.GetRelativePath(rawPath, file)}\" as it was disabled by config.");
					        continue;
				        }
				        
				        path = Path.GetDirectoryName(file);

				        Assembly[] result;
				        ProcessFile(path, file, out result);
				        processed++;

				        if (result == null)
					        continue;

				        foreach (var assembly in result)
				        {
					        if (!loadedAssemblies.ContainsKey(assembly))
						        loadedAssemblies.Add(assembly, path);
				        }
			        }
			        catch (BadImageFormatException ex)
			        {
				        if (Log.IsDebugEnabled)
					        Log.Debug($"File is not a .NET Assembly ({file})", ex);
			        }
			        catch (Exception ex)
			        {
				        Log.Error($"Failed loading \"{file}\"", ex);
			        }
		        }
	        }

	        Log.Info($"Loaded {loadedAssemblies.Count} assemblies from {processed} processed files.");

	        //List<OpenPlugin> plugins = new List<OpenPlugin>();
	        LinkedList<PluginConstructorData> constructorDatas = new LinkedList<PluginConstructorData>();
	        foreach (var assembly in loadedAssemblies)
	        {
		        if (assembly.Key != null)
		        {
			        var constructors = FindPluginConstructors(assembly.Key);
			        foreach (var constructor in constructors)
			        {
				        var existing = constructorDatas.FirstOrDefault(x => x.Type == constructor.Type);
				        if (existing != null)
				        {
					        if (!existing.ReferencesOtherPlugin && constructor.ReferencesOtherPlugin)
					        {
						        var found = constructorDatas.Find(existing);
						        if (found != null)
						        {
							        found.Value = constructor;
							        continue;
						        }
					        }
				        }

				        constructorDatas.AddLast(constructor);
			        }
		        }
	        }

	        Dictionary<Assembly, List<OpenPlugin>> assemblies = new Dictionary<Assembly, List<OpenPlugin>>();
	        //Load all plugins that do NOT have a reference to any other plugins.
	        foreach (var grouped in constructorDatas.Where(x => !x.ReferencesOtherPlugin).GroupBy(x => x.Type.Assembly))
	        {
		        List<OpenPlugin> assemblyInstances = new List<OpenPlugin>();
		        foreach (var constructor in grouped)
		        {
			        if (CreateInstance(constructor, out OpenPlugin instance, assemblies))
			        {
				        Services.RegisterSingleton(instance.GetType(), instance);
				        pluginInstances++;
				        
				        assemblyInstances.Add(instance);
			        }
		        }

		        if (!assemblies.ContainsKey(grouped.Key))
			        assemblies.Add(grouped.Key, assemblyInstances);
		        else
		        {
			        assemblies[grouped.Key].AddRange(assemblyInstances);
		        }
	        }

	        LinkedList<PluginConstructorData> ordered = new LinkedList<PluginConstructorData>();

	        var requiresOthers = constructorDatas.Where(x => x.ReferencesOtherPlugin).ToArray();
	        foreach (var grouped in requiresOthers)
	        {
		        var thisNode = ordered.Find(grouped);
		        if (thisNode == null)
		        {
			        thisNode = ordered.AddLast(grouped);
		        }

		        var otherPlugins = grouped.Dependencies.Where(x => x.IsPluginInstance).Select(x => x.Type).ToArray();
		        foreach (var otherDependency in otherPlugins)
		        {
			        var found = requiresOthers.FirstOrDefault(x => x.Type == otherDependency);
			        if (found != null)
						ordered.AddBefore(thisNode, found);
		        }
	        }

	        bool done = false;
	        
	        var current = ordered.First;
	        do
	        {
		        var currentValue = current?.Value;
		        var next = current?.Next;
		        if (next == null || currentValue == null)
		        {
			        done = true;
			        break;
		        }

		        if (currentValue.Requires(next.Value))
		        {
			        current.Value = next.Value;
			        next.Value = currentValue;
		        }

		        current = next;
	        } while (!done);

	        foreach (var item in ordered)
	        {
		        // List<OpenPlugin> assemblyInstances = new List<OpenPlugin>();

		        if (Services.TryResolve(item.Type, out _))
			        continue;
		        
		        if (CreateInstance(item, out var instance, assemblies))
		        {
			        Services.RegisterSingleton(item.Type, instance);//.Add(instance);
			        pluginInstances++;
			        
			        
			        if (!assemblies.ContainsKey(item.Type.Assembly))
				        assemblies.Add(item.Type.Assembly, new List<OpenPlugin>()
				        {
					        instance
				        });
			        else
			        {
				        assemblies[item.Type.Assembly].Add(instance);
			        }
		        }
	        }


	        Log.Info($"Registered {pluginInstances} plugin instances");

	        BuildDependencyGraph(constructorDatas, out var dependencies, out var dependents);

	        foreach (var grouped in assemblies)
	        {
		        LoadedAssemblies.Add(grouped.Key,
		            new LoadedAssembly(
			            grouped.Key,
			            grouped.Value.Select(x => x.GetType()),
			            dependencies.TryGetValue(grouped.Key, out var dependsOn) ? dependsOn : Enumerable.Empty<Assembly>(),
			            dependents.TryGetValue(grouped.Key, out var dependedOnBy) ? dependedOnBy : Enumerable.Empty<Assembly>(),
			            grouped.Key.Location));
	        }
        }

        /// <summary>
        /// 	Derives the inter-plugin assembly graph, in both directions, from the constructor
        /// 	parameters that ask for another plugin instance.
        /// </summary>
        /// <remarks>
        /// 	This information was previously computed here and then discarded — every
        /// 	<see cref="LoadedAssembly"/> was built with an empty reference list, which left the
        /// 	unload cascade unable to find any dependents and made
        /// 	<see cref="LoadedPlugin.Dependencies"/> permanently empty.
        /// </remarks>
        private static void BuildDependencyGraph(
	        IEnumerable<PluginConstructorData> constructorDatas,
	        out Dictionary<Assembly, HashSet<Assembly>> dependencies,
	        out Dictionary<Assembly, HashSet<Assembly>> dependents)
        {
	        dependencies = new Dictionary<Assembly, HashSet<Assembly>>();
	        dependents = new Dictionary<Assembly, HashSet<Assembly>>();

	        foreach (var constructor in constructorDatas)
	        {
		        Assembly dependent = constructor.Type.Assembly;

		        foreach (var parameter in constructor.Dependencies.Where(x => x.IsPluginInstance))
		        {
			        Assembly dependency = parameter.Type.Assembly;

			        // Plugins in the same assembly referencing each other is not an edge; it
			        // would make the assembly its own dependent and the cascade would recurse.
			        if (dependency == dependent)
				        continue;

			        if (!dependencies.TryGetValue(dependent, out var dependsOn))
				        dependencies[dependent] = dependsOn = new HashSet<Assembly>();
			        dependsOn.Add(dependency);

			        if (!dependents.TryGetValue(dependency, out var dependedOnBy))
				        dependents[dependency] = dependedOnBy = new HashSet<Assembly>();
			        dependedOnBy.Add(dependent);
		        }
	        }
        }

        private bool CreateInstance(PluginConstructorData constructorData, out OpenPlugin pluginInstance, Dictionary<Assembly, List<OpenPlugin>> assemblies)
	    {
		    List<object> parameters = new List<object>();
		    foreach (var param in constructorData.Dependencies)
		    {
			    if (Services.TryResolve(param.Type, out object parameter))
			    {
				    parameters.Add(parameter);
				    continue;
			    }

			    pluginInstance = null;
			    return false;
		    }

		    pluginInstance = (OpenPlugin) constructorData.Constructor.Invoke(parameters.ToArray());
		    return true;
	    }

	    internal void EnablePlugins()
	    {
		    int enabled = 0;
		    foreach (var type in LoadedAssemblies.Values.SelectMany(x => x.PluginTypes))
		    {
			    try
			    {
				    if (Services.TryResolve(type, out object pluginInstance))
				    {
					    var plugin = pluginInstance as OpenPlugin;
					    if (plugin == null)
						    continue;
				    
					    plugin.Enabled(Parent);

					    string authors = (plugin.Info.Authors == null || plugin.Info.Authors.Length == 0)
						    ? plugin.Info.Author
						    : string.Join(", ", plugin.Info.Authors);
				    
					    Log.Info($"Enabled '{plugin.Info.Name}' version {plugin.Info.Version} by {authors}");

					    Parent.EventDispatcher.DispatchEvent(new PluginEnabledEvent(type.Assembly, plugin));
					    
					    enabled++;
				    }
			    }
			    catch (Exception ex)
			    {
				    Log.Error($"Error occured while enabling plugin!", ex);
				    Services.Remove(type); //Could not enable plugin, so remove it from depency injection.
			    }
		    }

		    Log.Info($"Enabled {enabled} plugins!");
	    }

	    private bool ReferencesHost(ModuleDefinition assembly)
	    {
		    var hostName = HostAssembly.GetName();

			return assembly.AssemblyReferences
			    .Any(x => x.Name.Equals(hostName.Name, StringComparison.InvariantCultureIgnoreCase));
	    }

	    private bool ReferencesHost(Assembly assembly)
	    {
		    var hostName = HostAssembly.GetName();

		    return assembly.GetReferencedAssemblies()
			    .Any(x => x.Name.Equals(hostName.Name, StringComparison.InvariantCultureIgnoreCase));
	    }

	    private void ProcessFile(string directory, string file, out Assembly[] pluginAssemblies)
	    {
		    pluginAssemblies = null;

		    List<Assembly> assemblies = new List<Assembly>();


		    if (!File.Exists(file))
			    throw new FileNotFoundException("File not found: " + file);

		    try
		    {
			    var module = ModuleDefinition.ReadModule(file);

			    AssemblyNameReference assemblyName = module.Assembly.Name;
			    if (AssemblyManager.IsLoaded(assemblyName.Name, out _))
				    return;

			    if (!ReferencesHost(module))
				    return;

			    if (AssemblyResolver.TryResolve(directory, module, out Assembly[] loadedReferences))
			    {
				    foreach (var reference in loadedReferences)
				    {
					    if (!assemblies.Contains(reference) && ReferencesHost(reference))
					    {
						    assemblies.Add(reference);
					    }
				    }

				    if (AssemblyManager.TryLoadAssemblyFromFile(assemblyName.Name, file, out var result))
				    {
					    assemblies.Add(result);
				    }
			    }
			    else
			    {
				    Log.Warn($"Could not resolve all references for \"{module.Name}\"");
			    }
		    }
		    catch (Exception ex)
		    {
			    if (!(ex is BadImageFormatException))
				    Log.Error($"Could not load assembly as OpenPlugin (File: {file})", ex);
		    }
		    finally
		    {

		    }

		    pluginAssemblies = assemblies.ToArray();
	    }

	    private bool FindEmptyConstructor(Type type, out ConstructorInfo constructorInfo)
	    {
		    ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);
		    if (ctor != null)
		    {
			    constructorInfo = ctor;
			    return true;
		    }

		    constructorInfo = null;
		    return false;
	    }

	    private PluginConstructorData[] FindPluginConstructors(Assembly assembly)
	    {
		    List<PluginConstructorData> assemblyDatas = new List<PluginConstructorData>();
		    
		    Type[] types = assembly.GetExportedTypes();
		    foreach (Type type in types.Where(x => _openPluginType.IsAssignableFrom(x) && !x.IsAbstract && x.IsClass))
		    {
			    if (!Config.GetProperty($"plugin.{type.Name}.enabled", true))
			    {
				    Log.Info($"Not creating plugin instance off type \"{type.FullName}\" as it was disabled by config.");
				    continue;
			    }
			    
			    if (FindEmptyConstructor(type, out var constructorInfo))
			    {
				    assemblyDatas.Add(new PluginConstructorData(type, constructorInfo));
				    continue;
			    }
			    
			    foreach (ConstructorInfo constructor in type.GetConstructors())
			    {
				    var constructorParameters = constructor.GetParameters();
				    
				   // List<Assembly> assembliesReferenced = new List<Assembly>();
				    List<PluginConstructorData.ConstructorParameter> parameters = new List<PluginConstructorData.ConstructorParameter>();
				    foreach (ParameterInfo argument in constructorParameters)
				    {
					    if (argument.ParameterType == typeof(OpenApi))
					    {
						    parameters.Add(new PluginConstructorData.ConstructorParameter(typeof(OpenApi), false));
						    continue;
					    } 
					    else if (_openPluginType.IsAssignableFrom(argument.ParameterType))
					    {
						    parameters.Add(new PluginConstructorData.ConstructorParameter(argument.ParameterType, true));
					    }
				    }

				    if (parameters.Count == constructorParameters.Length)
				    {
					    assemblyDatas.Add(new PluginConstructorData(type, constructor)
					    {
						    Dependencies = parameters.ToArray()
					    });
					    break;
				    }
			    }
		    }

		    return assemblyDatas.ToArray();
	    }

	    private readonly Type _openPluginType = typeof(OpenPlugin);

	    /// <summary>
	    /// 	Unloads all plugins registered by specified assembly
	    /// </summary>
	    /// <param name="pluginAssembly"></param>
	    public void UnloadPluginAssembly(Assembly pluginAssembly)
        {
	        if (!LoadedAssemblies.TryGetValue(pluginAssembly, out LoadedAssembly assemblyPlugins))
	        {
		        Log.Error($"Error unloading all plugins for assembly: No plugins found/loaded.");
		        return;
	        }

	        // Dependents first. They hold this assembly's types and instances, so it cannot be
	        // released while any of them is still loaded — the reload unit is a plugin plus its
	        // transitive dependents. Snapshotted because the recursion mutates LoadedAssemblies.
	        foreach (Assembly dependent in assemblyPlugins.Dependents.ToArray())
	        {
		        if (LoadedAssemblies.ContainsKey(dependent))
		        {
			        UnloadPluginAssembly(dependent);
		        }
	        }

	        // Give each plugin a chance to release state the host cannot see (files, sockets,
	        // its own caches). Best-effort only — correctness is PurgeAssembly's job below.
	        foreach (var type in assemblyPlugins.PluginTypes.ToArray())
	        {
		        if (Services.TryResolve(type, out var instance) && instance is OpenPlugin plugin)
		        {
			        UnloadPlugin(plugin);
		        }
	        }

	        // Host-side teardown. Does not depend on plugin cooperation, and covers every
	        // registry rather than just the dependency container as this method used to.
	        PurgeAssembly(pluginAssembly);
        }

        /// <summary>
        /// 	Releases every host-side reference to types and instances belonging to
        /// 	<paramref name="assembly"/>, so that a collectible load context holding it
        /// 	can actually unload.
        /// </summary>
        /// <remarks>
        /// 	This is the single teardown entry point. Anything in the host that can hold a
        /// 	plugin instance, <see cref="Type"/>, <see cref="MethodInfo"/> or delegate must be
        /// 	purged from here — a single surviving reference silently prevents the assembly
        /// 	from ever being collected, with no exception and no log line.
        ///
        /// 	Teardown must not depend on plugins calling unregister methods in
        /// 	<see cref="OpenPlugin.Disabled"/>; several do not.
        /// </remarks>
        /// <param name="assembly">The plugin assembly to release.</param>
        public void PurgeAssembly(Assembly assembly)
        {
	        if (assembly == null) throw new ArgumentNullException(nameof(assembly));

	        // Every self-registered holder: the root dispatcher and tick scheduler, plus one of
	        // each per level. Driven from the registry rather than by walking the level manager
	        // so a level that was never registered is still reached.
	        var purgeables = Parent?.GetPurgeables();
	        if (purgeables != null)
	        {
		        foreach (var purgeable in purgeables)
		        {
			        purgeable.PurgeAssembly(assembly);
		        }
	        }

	        // Players outlive reloads too, and carry plugin-declared attribute types.
	        Parent?.PlayerManager?.PurgeAssembly(assembly);

	        Parent?.CommandManager?.PurgeAssembly(assembly);
	        Parent?.ItemFactory?.PurgeAssembly(assembly);
	        Services.PurgeAssembly(assembly);

	        LoadedAssemblies.Remove(assembly);
	        AssemblyManager.Remove(assembly);
        }

        private void UnloadPlugin(OpenPlugin plugin)
        {
            //lock (_pluginLock)
            {
                plugin.Disabled(Parent);

                string authors = (plugin.Info.Authors == null || plugin.Info.Authors.Length == 0)
	                ? plugin.Info.Author
	                : string.Join(", ", plugin.Info.Authors);

                Log.Info($"Disabled '{plugin.Info.Name}' version {plugin.Info.Version} by {authors}");
                
                Assembly assembly = plugin.GetType().Assembly;

                Parent?.EventDispatcher?.DispatchEvent(new PluginDisabledEvent(assembly, plugin));

	            if (LoadedAssemblies.TryGetValue(assembly, out LoadedAssembly assemblyPlugins))
	            {
		            Services.Remove(plugin.GetType());
		            assemblyPlugins.PluginTypes.Remove(plugin.GetType());
					Parent?.CommandManager.UnloadCommands(plugin);
					
		            if (!assemblyPlugins.PluginTypes.Any())
		            {
			            LoadedAssemblies.Remove(assembly);
		            }
				}
	            else
	            {
					Log.Error($"Error unloading plugin {plugin.GetType()}: Assembly has no loaded plugins");
	            }
            }
        }

        /// <summary>
        /// 	Unloads all loaded plugins
        /// </summary>
        public void UnloadAll()
        {
           // lock (_pluginLock)
            {
                foreach (var pluginAssembly in LoadedAssemblies.ToArray())
                {
	                UnloadPluginAssembly(pluginAssembly.Key);
                }
            }
        }

        /// <summary>
        /// 	Returns a list of all loaded plugins.
        /// </summary>
        /// <returns></returns>
        public LoadedPlugin[] GetLoadedPlugins()
	    {
		    return LoadedAssemblies.Values.SelectMany(x =>
		    {
			    string[] referencedPlugins = GetReferencedPlugins(x);
			    return x.PluginTypes.Select((type) =>
			    {
				    if (Services.TryResolve(type, out object instance) && instance is OpenPlugin p)
				    {
					    OpenPluginInfo info = p.Info;

					    return new LoadedPlugin(p, info, true)
					    {
						    Dependencies = referencedPlugins
					    };
				    }

				    return null;
			    }).Where(xx => xx != null);
		    }).ToArray();
	    }

	    private string[] GetReferencedPlugins(LoadedAssembly assembly)
	    {
			List<string> references = new List<string>();

		    // Forward direction: the plugins this one binds to. Not Dependents, which is what
		    // the unload cascade follows.
		    foreach (var asm in assembly.Dependencies)
		    {
			    if (LoadedAssemblies.TryGetValue(asm, out LoadedAssembly reference))
			    {
				    foreach (var plugin in reference.PluginTypes)
				    {
					    references.Add(plugin.AssemblyQualifiedName);
				    }
			    }
		    }

		    return references.ToArray();
	    }
	    
	    
	    private class PluginConstructorData
	    {
		    public Type Type { get; set; }
		    public ConstructorInfo Constructor { get; set; }
		    public ConstructorParameter[] Dependencies { get; set; } = new ConstructorParameter[0];
		    public bool ReferencesOtherPlugin => Dependencies.Any(x => x.IsPluginInstance);

		    public PluginConstructorData(Type pluginType, ConstructorInfo constructor)
		    {
			    Type = pluginType;
			    Constructor = constructor;
			    
		    }

		    public bool Requires(PluginConstructorData other)
		    {
			    return Dependencies.Any(x => x.Type == other.Type);
		    }
		    
		    public class ConstructorParameter
		    {
			    public Type Type { get; set; }
			    //public object Value { get; set; } = null;
			    public bool IsPluginInstance { get; set; }
			    
			    public ConstructorParameter(Type type, bool isPluginInstance)
			    {
				    Type = type;
				    //   Value = value;
				    IsPluginInstance = isPluginInstance;
			    }
		    }
	    }
	}
}
