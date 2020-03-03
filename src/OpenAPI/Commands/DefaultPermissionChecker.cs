using OpenAPI.Player;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAPI.Commands
{
    // TODO: Figure a better name then StringPermission
    public class DefaultPermissionChecker : CommandPermissionChecker<StringPermissionAttribute>
    {
        public override bool HasPermission(StringPermissionAttribute attr, OpenPlayer player)
        {
            return player.Permissions.HasPermission(attr.Permission);
        }
    }
}
