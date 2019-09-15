using MiNET.Plugins.Attributes;
using OpenAPI.Commands;
using OpenAPI.Permission;
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

        [Command(Description = "Make player a mod")]
        public void ModMe(OpenPlayer player)
        {
            PermissionGroup permissionGroup = new PermissionGroup("testperms");
            permissionGroup.SetPermission("testperms.mod", true);

            player.Permissions.AddPermissionGroup(permissionGroup);
            player.RefreshCommands();

            player.SendMessage($"You are a mod now!");
        }

        [Command(Description = "Are you a mod?", Permission = "testperms.mod")]
        public void AmIMod(OpenPlayer player)
        {
            player.SendMessage($"You are a mod!");
        }

        [Command(Description = "Are you a mod?")]
        [StringPermission("testperms.mod")]
        public void AmIMod2(OpenPlayer player)
        {
            player.SendMessage($"You are a mod!");
        }
    }
}
