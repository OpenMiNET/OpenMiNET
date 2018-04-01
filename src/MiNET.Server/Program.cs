using log4net;
using log4net.Config;
using MiNET;
using OpenAPI;
using Topshelf;

// Configure log4net using the .config file

[assembly: XmlConfigurator(Watch = true)]
// This will cause log4net to look for a configuration file
// called TestApp.exe.config in the application base
// directory (i.e. the directory containing TestApp.exe)
// The config file will be watched for changes.

namespace MiNET.Server
{
	class Program
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(Program));
		static void Main(string[] args)
		{
			HostFactory.Run(x =>
			{
				x.Service<OpenServer>(s =>
				{
					s.ConstructUsing(name => new OpenServer());
					s.WhenStarted(tc => tc.StartServer());
					s.WhenStopped(tc => tc.StopServer());
				});
				x.RunAsLocalSystem();

				x.SetDescription("OpenAPI MiNET Service");
				x.SetDisplayName("MiNET Service");
				x.SetServiceName("MiNET.Service");
			});
		}
	}
}
