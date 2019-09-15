using OpenAPI.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAPI.TestPlugin.FactionsExample
{
    public class FactionPermissionAttribute : CommandPermissionAttribute
    {
        public FactionsPermissions Permission { get; set; }

        public FactionPermissionAttribute(FactionsPermissions permission)
        {
            Permission = permission;
        }
    }
}
