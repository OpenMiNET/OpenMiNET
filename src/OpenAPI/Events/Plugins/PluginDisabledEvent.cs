using System.Reflection;
using OpenAPI.Plugins;

namespace OpenAPI.Events.Plugins
{
	/// <summary>
	///		Dispatched when a plugin gets disabled.
	/// </summary>
	public class PluginDisabledEvent : PluginEventBase
	{
		/// <summary>
		///     
		/// </summary>
		/// <param name="pluginAssembly"></param>
		/// <param name="pluginInstance"></param>
		public PluginDisabledEvent(Assembly pluginAssembly, OpenPlugin pluginInstance) : base(
			pluginAssembly, pluginInstance)
		{
			
		}
	}
}