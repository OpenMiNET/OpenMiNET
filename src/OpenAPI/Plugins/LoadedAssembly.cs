using System.Collections.Generic;
using System.Reflection;

namespace OpenAPI.Plugins
{
	public class LoadedAssembly
	{
	//	public PluginHost PluginHost { get; }
		public Assembly Assembly { get; }
		public List<OpenPlugin> PluginInstances { get; }
		public List<Assembly> AssemblyReferences { get; }
		public LoadedAssembly(/*PluginHost host,*/ Assembly assembly, IEnumerable<OpenPlugin> pluginInstances, IEnumerable<Assembly> referencedAssemblies)
		{
		//	PluginHost = host;
			Assembly = assembly;
			PluginInstances = new List<OpenPlugin>(pluginInstances);
			AssemblyReferences = new List<Assembly>(referencedAssemblies);
		}
	}
}