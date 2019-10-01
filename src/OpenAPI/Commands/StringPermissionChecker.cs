using OpenAPI.Player;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAPI.Commands
{
    // TODO: Figure a better name then StringPermission
    public class StringPermissionChecker : CommandPermissionChecker
    {
        public override bool HasPermission(CommandPermissionAttribute attr, OpenPlayer player)
        {
            if(! (attr is StringPermissionAttribute))
            {
                throw new InvalidOperationException("Invalid permission attribute.");
            }

            return player.Permissions.HasPermission((attr as StringPermissionAttribute).Permission);
        }
    }
}
