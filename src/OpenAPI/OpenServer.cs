using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MiNET;
using MiNET.Crafting;
using MiNET.Items;
using MiNET.Net;
using MiNET.Net.NetherNet;
using MiNET.Plugins;
using MiNET.Utils;
using MiNET.Utils.Diagnostics;
using MiNET.Utils.IO;
using OpenAPI.Events.Server;
using OpenAPI.Utils;

namespace OpenAPI
{
    public class OpenServer : MiNET.MiNetServer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(OpenServer));

        private OpenApi OpenApi { get; set; }
        public static DedicatedThreadPool FastThreadPool => ReflectionHelper.GetPrivateStaticPropertyValue<DedicatedThreadPool>(typeof(MiNetServer), "FastThreadPool");

        public EventHandler OnServerShutdown;
        private NetherNetListener _listener;
        public OpenServer()
        {
            OpenApi = new OpenApi();
        }

        /// <summary>
        ///     Starts the server
        /// </summary>
        /// <returns></returns>
        public new bool StartServer()
        {
            var type = typeof(MiNetServer);
            
            NetherNetListener c = ReflectionHelper.GetPrivateFieldValue<NetherNetListener>(type, this, "_netherNetListener");
            if (c != null) return false;

            try
            {
                Log.Info("Initializing...");

                if (ServerRole == ServerRole.Full || ServerRole == ServerRole.Proxy)
                {
                    if (Endpoint == null)
                    {
                        var ip = IPAddress.Parse(Config.GetProperty("ip", "0.0.0.0"));
                        int port = Config.GetProperty("port", 19132);
                        ReflectionHelper.SetPrivatePropertyValue(type, this, "Endpoint",
                            new IPEndPoint(ip, port));
                    }
                }

                ServerManager = ServerManager ?? new DefaultServerManager(this);
                OpenServerInfo openInfo = null;

                if (ServerRole == ServerRole.Full || ServerRole == ServerRole.Node)
                {
                    PluginManager = new PluginManager();

                    global::MiNET.Items.ItemFactory.CustomItemFactory = OpenApi.ItemFactory;

                    SessionManager = SessionManager ?? new SessionManager();
                    LevelManager = OpenApi.LevelManager;
                    PlayerFactory = OpenApi.PlayerManager;
                }

                MotdProvider = OpenApi.MotdProvider;
                if (Endpoint != null)
				{
					MotdProvider.PortV4 = Endpoint.Port;

					// Both the same, because one socket serves both families and nothing is bound on
					// port + 1.
					MotdProvider.PortV6 = Endpoint.Port;
				}

                OpenApi.OnEnable(this);

                // Load the recipe registry here, after plugins have had their say about it, so the
                // first player to join doesn't pay for it on the login thread (resolving thousands of
                // recipes takes about a second).
                Log.Info($"Loaded {RecipeManager.Recipes.Count} recipes");
                
                if (ServerRole == ServerRole.Full || ServerRole == ServerRole.Proxy)
                {
                    NetherNetListener listener = new NetherNetListener(Endpoint);
                    listener.CustomMessageHandlerFactory = session => new BedrockMessageHandler(session, ServerManager, PluginManager);

                    // Plugins serve the server port for anything NetherNet does not claim itself.
                    listener.RequestHandler = PluginManager.HandleHttpRequest;

                    openInfo = new OpenServerInfo(listener, OpenApi, OpenApi.LevelManager);

                    ConnectionInfo = openInfo;
                    openInfo.Init();

                    OpenApi.ServerInfo = openInfo;

                    // The same live count OpenServerInfo reports, handed to the meter so
                    // transport.sessions.active is the denominator for every per-session rate a
                    // collector computes.
                    TransportMetrics.SessionCountProvider = () => listener.Sessions.Count;
                    TransportMetrics.SendQueueDepthProvider = () =>
                    {
                        long depth = 0;
                        foreach (NetherNetSession session in listener.Sessions.Values) depth += session.SendQueueDepth;
                        return depth;
                    };
                    TransportMetrics.DispatchQueueDepthProvider = () =>
                    {
                        long depth = 0;
                        foreach (NetherNetSession session in listener.Sessions.Values) depth += session.DispatchQueueDepth;
                        return depth;
                    };

                    // The mux answers RakNet's legacy unconnected ping on the gameplay UDP port so
                    // the server still shows in the client's server tab; Mojang shipped no NetherNet
                    // replacement for it. EnableDiscovery=false turns the responder off entirely.
                    if (Config.GetProperty("EnableDiscovery", true))
                    {
                        listener.Discovery = new NetherNetDiscovery(MotdProvider, ConnectionInfo, () => listener.Sessions.Count);
                    }

                    ReflectionHelper.SetPrivateFieldValue(type, this, "_netherNetListener", listener);
                    listener.Start();

                    _listener = listener;
                }

                openInfo?.OnEnable();
                Log.Info("Server open for business on port " + Endpoint?.Port + " ...");

                OpenApi.EventDispatcher.DispatchEvent(new ServerReadyEvent(this));
                
                return true;
            }
            catch (Exception e)
            {
                Log.Error("Error during startup!", e);
                StopServer();
            }

            return false;
        }

        /// <summary>
        ///     Stops the server gracefully
        /// </summary>
        /// <returns></returns>
        public new bool StopServer()
        {
            OpenApi.EventDispatcher.DispatchEvent(new ServerClosingEvent(this));
            
            Log.Info($"Stopping server...");
            _listener?.Stop();
            var task = Task.Run(
                () =>
                {
                    try
                    {
                        OpenApi?.OnDisable();
                    }
                    finally
                    {
                        _listener?.Stop();
                        OnServerShutdown?.Invoke(this, EventArgs.Empty);
                    }
                });

            if (!task.Wait(Config.GetProperty("ForcedShutdownDelay", 10) * 1000))
            {
                Log.Warn($"Server took too long to shutdown, force exiting...");
                Environment.Exit(1);

                return false;
            }

            Log.Info($"Server shutdown gracefully... Exiting...");
            Environment.Exit(0);
            return true;
        }
    }
}
