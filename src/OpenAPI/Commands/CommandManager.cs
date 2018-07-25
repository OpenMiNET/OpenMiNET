namespace OpenAPI.Commands
{
    public class CommandManager
    {
		private MiNET.Plugins.PluginManager PluginManager { get; }
		public CommandManager(MiNET.Plugins.PluginManager pluginManager)
		{
			PluginManager = pluginManager;
		}

	    public void LoadCommands(object instance)
	    {
			PluginManager.LoadCommands(instance);
	    }

	    public void UnloadCommands(object instance)
	    {
		    PluginManager.UnloadCommands(instance);
		}
	}
}
