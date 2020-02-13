using System;
using log4net;
using OpenAPI.Plugins;
using OpenAPI.TestPlugin;

namespace CrossReferencing
{
	[OpenPluginInfo(Name = "OpenAPI CrossReferencing Example", Description = "An example plugin showing the ability to cross-reference another plugin", Author = "Kenny van Vulpen", Version = "1.0", Website = "https://github.com/OpenMiNET/OpenAPI")]
    public class ExamplePlugin : OpenPlugin
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(ExamplePlugin));

		private TestPlugin OtherPlugin { get; }

		//Here you can see that the constructor for ExamplePlugin requires an instance of TestPlugin to be passed on, this allows us to communicate between plugins.
		public ExamplePlugin(TestPlugin testPlugin)
		{
			OtherPlugin = testPlugin;
			
		}

		public override void Enabled(OpenAPI.OpenAPI api)
		{
			Log.InfoFormat("Example plugin enabled!");
			OtherPlugin.HelloWorld("ExamplePlugin says hi!");
		}

		public override void Disabled(OpenAPI.OpenAPI api)
		{
			Log.InfoFormat("Example plugin disabled");
		}
	}
}
