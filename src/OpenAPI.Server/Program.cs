using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml;
using log4net;
using log4net.Config;
using MiNET.Blocks;
using OpenAPI;

namespace OpenAPI.Server
{
    class Program
    {
		private static readonly log4net.ILog log =
			log4net.LogManager.GetLogger(typeof(Program));

		static void Main(string[] args)
        {
	        XmlDocument log4netConfig = new XmlDocument();
	        log4netConfig.Load(File.OpenRead("log4net.config"));

	        var repo = log4net.LogManager.CreateRepository(
		        Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));

	        log4net.Config.XmlConfigurator.Configure(repo, log4netConfig["log4net"]);

			int threads;
	        int portThreads;
	        ThreadPool.GetMinThreads(out threads, out portThreads);
	        
	        log.Info($"Threads: {threads}, Port Threads: {portThreads}");

	        var service = new OpenServer();
	        log.Info("Starting MiNET");
	        service.StartServer(); 

	        System.Console.WriteLine("MiNET is now running. Hold <CTRL+C> to stop the server gracefully.");
			ConsoleHost.WaitForShutdown();

	        System.Console.WriteLine("Stopping server...");

			service.StopServer();
		}
    }
}
