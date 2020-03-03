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
    
    public class CommandPermissionChecker<TType> : CommandPermissionChecker where TType : CommandPermissionAttribute
    {
        public virtual bool HasPermission(TType attr, OpenPlayer player)
        {
            throw new NotImplementedException();
        }

        public override bool HasPermission(CommandPermissionAttribute attr, OpenPlayer player)
        {
            if (attr is TType cmdAttribute)
                return HasPermission(cmdAttribute, player);
            
            throw new InvalidOperationException("The attribute type does not match the expected type!");
        }
    }
}
