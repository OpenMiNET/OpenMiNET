using MiNET.Plugins.Attributes;
using OpenAPI.Player;

namespace OpenAPI.TestPlugin
{
    public class ExampleCommands
    {
	    [Command(Description = "An example command to show how things work around OpenAPI")]
	    public void ExampleCommand(OpenPlayer player)
	    {
			player.SendMessage($"Hi! We got it!");
	    }
    }
}
