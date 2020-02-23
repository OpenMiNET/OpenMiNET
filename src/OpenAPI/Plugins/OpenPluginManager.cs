using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using Mono.Cecil;

namespace OpenAPI.Plugins
{
    public class OpenPluginManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(OpenPluginManager));
        
        private Dictionary<Assembly, LoadedAssembly> LoadedAssemblies { get; }
		private ConcurrentDictionary<Type, object> References { get; }
		
		private OpenApi Parent { get; }
		private Assembly HostAssembly { get; }
		private AssemblyManager AssemblyManager { get; }
		private AssemblyResolver AssemblyResolver { get; }
        public OpenPluginManager(OpenApi parent)
        {
            Parent = parent;
			HostAssembly = Assembly.GetAssembly(typeof(OpenPluginManager));
			
            LoadedAssemblies = new Dictionary<Assembly, LoadedAssembly>();
			References = new ConcurrentDictionary<Type, object>();
			
			AssemblyManager = new AssemblyManager();
			AssemblyResolver = new AssemblyResolver(AssemblyManager);
        }

        public void DiscoverPlugins(params string[] paths)
        {
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

	        List<OpenPlugin> plugins = new List<OpenPlugin>();
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
				        plugins.Add(instance);

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

		        OpenPlugin instance = plugins.FirstOrDefault(x => x.GetType() == item.Type);
		        if (instance != null)
			        continue;

		        if (CreateInstance(item, out instance, assemblies))
		        {
			        plugins.Add(instance);
			        
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


	        Log.Info($"Registered {plugins.Count} plugin instances");

	        foreach (var grouped in assemblies)
	        {
		        LoadedAssemblies.Add(grouped.Key,
		            new LoadedAssembly(grouped.Key, grouped.Value, new Assembly[0], grouped.Key.Location));
	        }
        }

        private bool CreateInstance(PluginConstructorData constructorData, out OpenPlugin pluginInstance, Dictionary<Assembly, List<OpenPlugin>> assemblies)
	    {
		    List<object> parameters = new List<object>();
		    foreach (var param in constructorData.Dependencies)
		    {
			//    Log.Info($"Need: {param.Type} | Plugin Instance? {param.IsPluginInstance}");
			    if (param.IsPluginInstance)
			    {
				    if (!assemblies.TryGetValue(param.Type.Assembly, out var loadedAssembly))
					    throw new Exception();

				    var instance = loadedAssembly.FirstOrDefault(x => x.GetType() == param.Type);
				    parameters.Add(instance);
				    continue;
			    }
			    
			    if (param.Value == null)
				    throw new Exception("Null value.");
				
			    parameters.Add(param.Value);
		    }

		    pluginInstance = (OpenPlugin) constructorData.Constructor.Invoke(parameters.ToArray());
		    return true;
	    }

	    internal void EnablePlugins()
	    {
		    int enabled = 0;
		    foreach (var plugin in LoadedAssemblies.Values.SelectMany(x => x.PluginInstances))
		    {
			    try
			    {
				    plugin.Enabled(Parent);
				    enabled++;

				    string authors = (plugin.Info.Authors == null || plugin.Info.Authors.Length == 0)
					    ? plugin.Info.Author
					    : string.Join(", ", plugin.Info.Authors);
				    
				    Log.Info($"Enabled '{plugin.Info.Name}' version {plugin.Info.Version} by {authors}");
			    }
			    catch (Exception ex)
			    {
				    Log.Error($"Error occured while enabling plugin!", ex);
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

	    private class PluginConstructorData
	    {
		    public Type Type { get; set; }
		    public ConstructorInfo Constructor { get; set; }
		    public ConstructorParameter[] Dependencies { get; set; } = new ConstructorParameter[0];
		    public bool ReferencesOtherPlugin => Dependencies.Any(x => x.IsPluginInstance && x.Value == null);

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
			    public object Value { get; set; } = null;
			    public bool IsPluginInstance { get; set; }
			    
			    public ConstructorParameter(Type type, object value, bool isPluginInstance)
			    {
				    Type = type;
				    Value = value;
				    IsPluginInstance = isPluginInstance;
			    }
		    }
	    }

	    private PluginConstructorData[] FindPluginConstructors(Assembly assembly)
	    {
		    List<PluginConstructorData> assemblyDatas = new List<PluginConstructorData>();
		    
		    Type[] types = assembly.GetExportedTypes();
		    foreach (Type type in types.Where(x => _openPluginType.IsAssignableFrom(x) && !x.IsAbstract && x.IsClass))
		    {
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
						    parameters.Add(new PluginConstructorData.ConstructorParameter(typeof(OpenApi), Parent, false));
						    continue;
					    } 
					    else if (_openPluginType.IsAssignableFrom(argument.ParameterType))
					    {
						    parameters.Add(new PluginConstructorData.ConstructorParameter(argument.ParameterType, null, true));
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
	    private bool LoadAssembly(Assembly assembly, out OpenPlugin[] loaded, out Assembly[] referencedAssemblies)
	    {
		    try
		    {
				var refAssemblies = new List<Assembly>();
			    var plugins = new List<OpenPlugin>();

			    Type[] types = assembly.GetExportedTypes();
			    foreach (Type type in types.Where(x => _openPluginType.IsAssignableFrom(x) && !x.IsAbstract && x.IsClass))
			    {
				    ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);
				    if (ctor != null)
				    {
					    OpenPlugin plugin;
					    try
					    {
						    plugin = (OpenPlugin) ctor.Invoke(null);
					    }
					    catch(Exception ex)
					    {
						    plugin = null;
						    Log.Error("An error has occurred", ex);
					    }

					    if (plugin != null)
					    {
							plugins.Add(plugin);
						}
				    }
				    else
				    {
					    foreach (ConstructorInfo constructor in type.GetConstructors())
					    {
							List<Assembly> assembliesReferenced = new List<Assembly>();
							List<object> parameters = new List<object>();
						    foreach (ParameterInfo argument in constructor.GetParameters())
						    {
							    if (argument.ParameterType == typeof(OpenApi))
							    {
								    parameters.Add(Parent);
									continue;
							    }

							    if (References.TryGetValue(argument.ParameterType, out object arg))
							    {
									parameters.Add(arg);

								    Assembly argsAssembly = arg.GetType().Assembly;
								    if (!assembliesReferenced.Contains(argsAssembly))
								    {
									    assembliesReferenced.Add(argsAssembly);
								    }
								    continue;
							    }

							    foreach (LoadedAssembly loadedAssembly in LoadedAssemblies.Values)
							    {
								    foreach (OpenPlugin loadedPlugin in loadedAssembly.PluginInstances)
								    {
									    if (argument.ParameterType == loadedPlugin.GetType())
									    {
										    parameters.Add(loadedPlugin);

										    if (loadedAssembly.Assembly != assembly) //If the instance of the type is not from the assembly being loaded, add the type's assembly to a list of dependencies
										    {
											    if (!assembliesReferenced.Contains(loadedAssembly.Assembly))
											    {
												    assembliesReferenced.Add(loadedAssembly.Assembly);
											    }
										    }
									    }
								    }
							    }
						    }

						    if (parameters.Count == constructor.GetParameters().Length)
						    {
							    var plugin = (OpenPlugin) constructor.Invoke(parameters.ToArray());
							    foreach (Assembly reference in assembliesReferenced)
							    {
								    if (!refAssemblies.Contains(reference))
								    {
									    refAssemblies.Add(reference);
								    }
							    }

								plugins.Add(plugin);
								//Log.Info($"Plugin instance created: {plugin.GetType().FullName}");
							    break;
							}
						    else
						    {
								Log.Warn($"Could not call constructor for {constructor.ToString()}");
						    }
					    }
				    }
			    }

			    if (plugins.Count > 0)
			    {
				    referencedAssemblies = refAssemblies.ToArray();
				    loaded = plugins.ToArray();
				    return true;
			    }
		    }
		    catch(Exception ex)
		    {
			    Log.Error($"Could not load assembly ({assembly.FullName})", ex);
		    }

			loaded = new OpenPlugin[0];
			referencedAssemblies = new Assembly[0];
		    return false;
	    }

	    public void UnloadPluginAssembly(Assembly pluginAssembly)
        {
           // lock (_pluginLock)
            {
	            if (!LoadedAssemblies.TryGetValue(pluginAssembly, out LoadedAssembly assemblyPlugins))
                {
                    Log.Error($"Error unloading all plugins for assembly: No plugins found/loaded.");
                    return;
                }

	            //Unload all assemblies that referenced this plugin's assembly
	            foreach (Assembly referencedAssembly in assemblyPlugins.AssemblyReferences)
	            {
		            if (LoadedAssemblies.ContainsKey(referencedAssembly))
		            {
			            UnloadPluginAssembly(referencedAssembly);
		            }
	            }

				//Remove all this assembly's type instances from list of references
	            foreach (Type type in pluginAssembly.GetTypes())
	            {
		            if (References.ContainsKey(type))
		            {
			            References.TryRemove(type, out var _);
		            }
	            }

				//Unload all plugin instances
				foreach (OpenPlugin plugin in assemblyPlugins.PluginInstances)
                {
                    UnloadPlugin(plugin);
                }
            }
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

	            if (LoadedAssemblies.TryGetValue(assembly, out LoadedAssembly assemblyPlugins))
	            {
		            assemblyPlugins.PluginInstances.Remove(plugin);
					Parent.CommandManager.UnloadCommands(plugin);
					
		            if (!assemblyPlugins.PluginInstances.Any())
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

        public void UnloadAll()
        {
           // lock (_pluginLock)
            {
                foreach (var pluginAssembly in LoadedAssemblies.ToArray())
                {
	                foreach (var pluginInstance in pluginAssembly.Value.PluginInstances)
	                {
		                UnloadPlugin(pluginInstance);
	                }
                  /*  if (LoadedAssemblies.TryGetValue(pluginAssembly.Value, out LoadedAssembly assembly))
                    {
                        foreach (OpenPlugin pluginInstance in assembly.PluginInstances)
                        {
	                        UnloadPlugin(pluginInstance);
                        }
                     //   LoadedAssemblies.Remove(pluginAssembly.Value);
                    }

                    AssemblyReferences.TryRemove(pluginAssembly.Key, out Assembly _);*/
                }
            }
        }

	    public void SetReference<TType>(TType reference)
	    {
		    if (!References.TryAdd(typeof(TType), reference))
		    {
			    throw new Exception("Type reference already set!");
		    }
	    }

	    public bool TryGetReference(Type type, out object result)
	    {
		    return References.TryGetValue(type, out result);
	    }
	    
	    public bool TryGetReference<TType>(out TType result)
	    {
		    if (References.TryGetValue(typeof(TType), out object value))
		    {
			    result = (TType) value;
			    return true;
		    }

		    result = default(TType);
		    return false;
	    }

	    public LoadedPlugin[] GetLoadedPlugins()
	    {
		    return LoadedAssemblies.Values.SelectMany(x =>
		    {
			    string[] referencedPlugins = GetReferencedPlugins(x);
			    return x.PluginInstances.Select((p) =>
			    {
				    OpenPluginInfo info = p.Info;

				    return new LoadedPlugin(p, info, true)
				    {
					    Dependencies = referencedPlugins
					};
			    });
		    }).ToArray();
	    }

	    private string[] GetReferencedPlugins(LoadedAssembly assembly)
	    {
			List<string> references = new List<string>();

		    foreach (var asm in assembly.AssemblyReferences)
		    {
			    if (LoadedAssemblies.TryGetValue(asm, out LoadedAssembly reference))
			    {
				    foreach (var plugin in reference.PluginInstances)
				    {
					    references.Add(plugin.GetType().AssemblyQualifiedName);
				    }
			    }
		    }

		    return references.ToArray();
	    }
	}
}
