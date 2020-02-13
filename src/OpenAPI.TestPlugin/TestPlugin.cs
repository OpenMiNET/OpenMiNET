using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using log4net;
using OpenAPI.Plugins;
using OpenAPI.TestPlugin.FactionsExample;

namespace OpenAPI.TestPlugin
{
	[OpenPluginInfo(Name = "OpenAPI TestPlugin", Description = "An example plugin", Author = "Kenny van Vulpen", Version = "1.0", Website = "https://github.com/OpenMiNET/OpenAPI")]
	public class TestPlugin : OpenPlugin
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(TestPlugin));

		private ExampleCommands CommandsClass { get; }

        private FactionManager FactionManager = new FactionManager();

		public TestPlugin()
		{
			CommandsClass = new ExampleCommands();
		}

		public override void Enabled(OpenAPI api)
		{
            api.CommandManager.RegisterPermissionChecker(typeof(FactionPermissionAttribute), new FactionPermissionChecker(FactionManager));

			api.CommandManager.LoadCommands(CommandsClass);
            api.CommandManager.LoadCommands(new FactionCommands(FactionManager));

			Log.InfoFormat("TestPlugin Enabled");
		}

		public override void Disabled(OpenAPI api)
		{
			api.CommandManager.UnloadCommands(CommandsClass);
			Log.InfoFormat("TestPlugin Disabled");
		}

		public void HelloWorld(string message, [CallerMemberName] string memberName = "")
		{
			StackTrace stackTrace = new StackTrace();
			var method = stackTrace.GetFrame(1).GetMethod();
			Log.Info($"[TestPlugin] {(method.DeclaringType.FullName)}.{method.Name}: " + message);
		}
	}
}
