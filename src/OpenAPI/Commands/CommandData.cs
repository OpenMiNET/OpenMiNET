using MiNET.Plugins.Attributes;

namespace OpenAPI.Commands
{
    public class CommandData
    {
        public CommandAttribute Attribute { get; set; }
        public object Instance { get; set; }

        public CommandData(CommandAttribute attribute, object instance)
        {
            Attribute = attribute;
            Instance = instance;
        }
    }
}