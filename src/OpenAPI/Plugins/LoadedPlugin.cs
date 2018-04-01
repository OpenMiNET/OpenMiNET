namespace OpenAPI.Plugins
{
	public class LoadedPlugin
	{
		public bool Enabled { get; }
		public OpenPluginInfo Info { get; }
		public OpenPlugin Instance { get; }
		public string[] Dependencies;
		internal LoadedPlugin(OpenPlugin pluginInstance, OpenPluginInfo info, bool enabled)
		{
			Instance = pluginInstance;
			Enabled = enabled;
			Info = info;
		}
	}
}