using OpenAPI.Player;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAPI.TestPlugin.FactionsExample
{
    public class FactionManager
    {
        private Dictionary<OpenPlayer, FactionsPermissions> PlayerFactionPermissions = new Dictionary<OpenPlayer, FactionsPermissions>();

        public void SetPlayerPermission(OpenPlayer player, FactionsPermissions permission)
        {
            PlayerFactionPermissions[player] = permission;
            player.RefreshCommands();
        }

        public FactionsPermissions GetPlayerPermission(OpenPlayer player)
        {
            if (!PlayerFactionPermissions.ContainsKey(player)) return FactionsPermissions.None;

            return PlayerFactionPermissions[player];
        }
    }
}
