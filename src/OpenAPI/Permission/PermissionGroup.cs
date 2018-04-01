using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace OpenAPI.Permission
{
    public class PermissionGroup : IEnumerable<KeyValuePair<string, bool>>
    {
        public string Name { get; }
        public bool Dirty { get; private set; }
        private ConcurrentDictionary<string, bool> Permissions = new ConcurrentDictionary<string, bool>();

        public PermissionGroup(string name)
        {
            this.Name = name;  
        }

        public void SetPermission(string permission, bool value)
        {
            this.Permissions[permission] = value;
        }

        public void RemovePermission(string permission)
        {
            if (this.Permissions.TryRemove(permission, out bool _))
            {
                this.Dirty = true;
            }
        }

        public bool this[string permission]
        {
            get
            {
                if (Permissions.TryGetValue(permission.ToLowerInvariant(), out bool value))
                {
                    return value;
                }

                return false;
            }
            set
            {
                Permissions.AddOrUpdate(permission.ToLowerInvariant(), s => value, (s, v) => value);
                Dirty = true;
            }
        }

        public bool Contains(string permissionIntern)
        {
            return Permissions.ContainsKey(permissionIntern.ToLowerInvariant());
        }

        IEnumerator<KeyValuePair<string, bool>> IEnumerable<KeyValuePair<string, bool>>.GetEnumerator()
        {
            foreach (var permission in Permissions.ToArray())
            {
                yield return permission;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, bool>>)(this)).GetEnumerator();
        }
    }
}
