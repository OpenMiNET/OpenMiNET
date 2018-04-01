using System.Collections.Generic;
using System.Linq;
using OpenAPI.Utils;

namespace OpenAPI.Permission
{
    public class PermissionManager
    {
        private ThreadSafeList<PermissionGroup> Groups { get; } = new ThreadSafeList<PermissionGroup>();
        public bool Dirty { get; set; }

        private PermissionGroup Cache { get; } = new PermissionGroup("Cache");
        public void Update()
        {

        }

        public bool HasPermission(string permission)
        {
            // Check if we have a cached copy
            string permissionIntern = permission.ToLowerInvariant();

            if (!this.Cache.Contains(permissionIntern))
            {
                // Check player permissions first
               
                string wildCardFound = null;

                foreach (var cursor in Groups.ToArray())
                {
                    foreach (KeyValuePair<string, bool> playerCursor in cursor)
                    {
                        string currentChecking = playerCursor.Key;

                        if (permissionIntern == currentChecking)
                        {
                            this.Cache[permissionIntern] = cursor[currentChecking];
                            return cursor[currentChecking];
                        }

                        // Check for wildcard
                        if (currentChecking.EndsWith("*"))
                        {
                            string wildCardChecking = currentChecking.Substring(0, currentChecking.Length - 1);
                            if (permissionIntern.StartsWith(wildCardChecking))
                            {
                                if (wildCardFound == null || currentChecking.Length > wildCardFound.Length)
                                {
                                    wildCardFound = currentChecking;
                                }
                            }
                        }
                    }
                }

                // Check if we found a wildcard
                if (wildCardFound != null)
                {
                    bool val = HasPermission(wildCardFound);
                    this.Cache[permissionIntern] = val;
                    return val;
                }

                return false;
            }

            return Cache[permissionIntern];
        }
    }
}
