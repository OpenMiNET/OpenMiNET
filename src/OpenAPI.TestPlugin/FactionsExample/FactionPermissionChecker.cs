using OpenAPI.Commands;
using OpenAPI.Player;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAPI.TestPlugin.FactionsExample
{
    public class FactionPermissionChecker : CommandPermissionChecker
    {
        private FactionManager Manager { get; set; }

        public FactionPermissionChecker(FactionManager manager)
        {
            Manager = manager;
        }

        public override bool HasPermission(CommandPermissionAttribute attr, OpenPlayer player)
        {
            if (!(attr is FactionPermissionAttribute))
            {
                throw new InvalidOperationException("Invalid permission attribute.");
            }

            return Manager.GetPlayerPermission(player) == ((attr as FactionPermissionAttribute).Permission);
        }
    }
}
