# Permissions

Implementing simple permissions for commands etc is fairly easy in OpenApi. 

You register your own PermissionChecker with the CommandManager and you are pretty much ready to go.

## Example:

### The permission checker class:

```C#

public class MyCoolPermissionChecker : CommandPermissionChecker<StringPermissionAttribute>
{
    public override bool HasPermission(StringPermissionAttribute attr, OpenPlayer player)
    {
        return player.Permissions.HasPermission(attr.Permission);
    }
}

```

### Your plugin class

```C#

public override void Enabled(OpenApi api)
{
    api.CommandManager.RegisterPermissionChecker(new MyCoolPermissionChecker());
}

```