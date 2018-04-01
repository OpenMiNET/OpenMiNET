using System;

namespace OpenAPI.Plugins
{
	[Serializable]
	public class PluginException : Exception
	{
		public PluginException() { }
		public PluginException(Exception ex) : base("Plugin Exception", ex) { }
	}
}
