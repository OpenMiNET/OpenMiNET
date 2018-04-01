using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
