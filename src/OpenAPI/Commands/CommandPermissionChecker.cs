using OpenAPI.Player;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAPI.Commands
{
    public abstract class CommandPermissionChecker
    {
        public abstract bool HasPermission(CommandPermissionAttribute attr, OpenPlayer player);
    }
}
