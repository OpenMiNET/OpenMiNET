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

		public override void Enabled(OpenAPI api)
		{
			api.PluginManager.SetReference<TestPlugin>(this); //By calling SetReference other plugins can talk to us. See the example "CrossReferencing"
			Log.InfoFormat("TestPlugin Enabled");
		}

		public override void Disabled(OpenAPI api)
		{
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
