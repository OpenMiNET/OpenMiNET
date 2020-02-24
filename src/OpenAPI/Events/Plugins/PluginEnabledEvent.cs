using System.Reflection;
using OpenAPI.Plugins;

namespace OpenAPI.Events.Plugins
{
    /// <summary>
    ///     Gets dispatched when a plugin was loaded & enabled
    /// </summary>
    public class PluginEnabledEvent : Event
    {
        /// <summary>
        ///     The assembly the plugin was loaded from
        /// </summary>
        public Assembly PluginAssembly { get; set; }
        
        /// <summary>
        ///     The plugin instance that was enabled
        /// </summary>
        public OpenPlugin PluginInstance { get; set; }
        
        /// <summary>
        ///     
        /// </summary>
        /// <param name="pluginAssembly"></param>
        /// <param name="pluginInstance"></param>
        public PluginEnabledEvent(Assembly pluginAssembly, OpenPlugin pluginInstance)
        {
            PluginAssembly = pluginAssembly;
            PluginInstance = pluginInstance;
        }
    }
}