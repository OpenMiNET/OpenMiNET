using OpenAPI.Plugins;

namespace OpenAPI.ManagementApi.Models.Plugins
{
    public class PluginInfo
    {
        public bool Enabled { get; set; }
        public OpenPluginInfo Info { get; set; }

        public PluginInfo(LoadedPlugin plugin)
        {
            Enabled = plugin.Enabled;
            Info = plugin.Info;
        }
    }
}