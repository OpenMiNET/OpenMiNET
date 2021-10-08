using System.Reflection;
using OpenAPI.Plugins;

namespace OpenAPI.Events.Plugins
{
    /// <summary>
    ///     Gets dispatched when a plugin was loaded & enabled
    /// </summary>
    public class PluginEnabledEvent : PluginEventBase
    {
        /// <summary>
        ///     
        /// </summary>
        /// <param name="pluginAssembly"></param>
        /// <param name="pluginInstance"></param>
        public PluginEnabledEvent(Assembly pluginAssembly, OpenPlugin pluginInstance) : base(pluginAssembly, pluginInstance)
        {
        }
    }
}