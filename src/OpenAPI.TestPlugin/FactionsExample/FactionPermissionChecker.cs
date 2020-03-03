using OpenAPI.Commands;
using OpenAPI.Player;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAPI.TestPlugin.FactionsExample
{
    public class FactionPermissionChecker : CommandPermissionChecker<FactionPermissionAttribute>
    {
        private FactionManager Manager { get; set; }

        public FactionPermissionChecker(FactionManager manager)
        {
            Manager = manager;
        }

        public override bool HasPermission(FactionPermissionAttribute attr, OpenPlayer player)
        {
            return Manager.GetPlayerPermission(player) == ((attr as FactionPermissionAttribute).Permission);
        }
    }
}
