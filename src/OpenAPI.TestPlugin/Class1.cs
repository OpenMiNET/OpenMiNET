using System;
using log4net;
using OpenAPI.Plugins;

namespace OpenAPI.TestPlugin
{
	public class TestPlugin : OpenPlugin
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(TestPlugin));

		public override void Enabled(OpenAPI api)
		{
			Log.InfoFormat("TestPlugin Enabled");
		}

		public override void Disabled(OpenAPI api)
		{
			Log.InfoFormat("TestPlugin Disabled");
		}
	}
}
