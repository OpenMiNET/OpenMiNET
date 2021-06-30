using System;
using MiNET;
using MiNET.Plugins.Attributes;
using MiNET.Sounds;
using MiNET.Worlds;
using OpenAPI.Commands;
using OpenAPI.Permission;
using OpenAPI.Player;

namespace OpenAPI.TestPlugin
{
    public class ExampleCommands
    {
        [Command(Description = "An example command showing custom stringenums.")]
        public void TestEnum(OpenPlayer player, CustomDynamicEnum test)
        {
            player.SendMessage($"You entered: {test.Value}");
        }
        
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

        [Command(Name = "sound")]
        public void Sound(OpenPlayer player)
        {
            player.SendSound(player.KnownPosition.GetCoordinates3D(), LevelSoundEventType.Explode);
            player.SendMessage("Sound send.");
        }
        
        [Command(Name = "sound2")]
        public void Sound2(OpenPlayer player, LevelSoundEventType sound)
        {
            //player.Level.MakeSound(new DoorCloseSound(player.KnownPosition));

            player.Level.BroadcastSound(player.KnownPosition.GetCoordinates3D(), sound);
            
          //  player.SendSound(player.KnownPosition.GetCoordinates3D(), LevelSoundEventType.Explode);
            player.SendMessage("Sound send.");
        }

        [Command(Name = "gm")]
        public void Gamemode(OpenPlayer player, GameMode gameMode)
        {
            player.SetGamemode(gameMode);
            player.SendMessage($"Gamemode set to: {gameMode}");
        }

        [Command(Name = "broadcast")]
        public void Broadcast(OpenPlayer source, OpenApi api, params string[] message)
        {
            string msg = String.Join(" ", message);

            foreach (var lvl in api.LevelManager.GetLevels)
            {
                lvl.BroadcastMessage(msg, MessageType.Chat, source);
            }
            
            source.SendMessage($"We tried broadcasting: {msg}");
        }
    }
}
