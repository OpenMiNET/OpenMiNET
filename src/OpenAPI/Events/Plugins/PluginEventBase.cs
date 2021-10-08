using System.Reflection;
using OpenAPI.Plugins;

namespace OpenAPI.Events.Plugins
{
	/// <summary>
	///		The base class for any plugin events
	/// </summary>
	public class PluginEventBase : Event
	{
		/// <summary>
		///     The assembly the plugin was loaded from
		/// </summary>
		public Assembly PluginAssembly { get; set; }

		/// <summary>
		///     The plugin instance
		/// </summary>
		public OpenPlugin PluginInstance { get; set; }

		/// <summary>
		///     
		/// </summary>
		/// <param name="pluginAssembly"></param>
		/// <param name="pluginInstance"></param>
		protected PluginEventBase(Assembly pluginAssembly, OpenPlugin pluginInstance)
		{
			PluginAssembly = pluginAssembly;
			PluginInstance = pluginInstance;
		}
	}
}