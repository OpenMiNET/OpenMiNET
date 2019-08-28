using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using log4net;
using OpenAPI.Plugins;

namespace OpenAPI.TestPlugin
{
	public class TestPlugin : OpenPlugin
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(TestPlugin));

		private ExampleCommands CommandsClass { get; }

		public TestPlugin()
		{
			CommandsClass = new ExampleCommands();
		}

		public override void Enabled(OpenAPI api)
		{
			api.CommandManager.LoadCommands(CommandsClass);

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
