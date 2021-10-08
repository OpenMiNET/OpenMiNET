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
using MiNET.Items;
using MiNET.Net;
using MiNET.Net.RakNet;
using MiNET.Plugins;
using MiNET.Utils;
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
        private RakConnection _rakListener;
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
            
            RakConnection c = ReflectionHelper.GetPrivateFieldValue<RakConnection>(type, this, "_listener");
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

                    GreyListManager = GreyListManager ?? new GreyListManager(ConnectionInfo);
                    SessionManager = SessionManager ?? new SessionManager();
                    LevelManager = OpenApi.LevelManager;
                    PlayerFactory = OpenApi.PlayerManager;
                }

                MotdProvider = OpenApi.MotdProvider;

                OpenApi.OnEnable(this);

                if (ServerRole == ServerRole.Full || ServerRole == ServerRole.Proxy)
                {
                    RakConnection listener = new RakConnection(Endpoint, GreyListManager, MotdProvider);
                    listener.CustomMessageHandlerFactory = session => new BedrockMessageHandler(session, ServerManager, PluginManager);

                   openInfo = new OpenServerInfo(listener, OpenApi,
                       listener.ConnectionInfo.RakSessions, OpenApi.LevelManager);
                    

                   ConnectionInfo = openInfo;
                   openInfo.Init();

                   OpenApi.ServerInfo = openInfo;
                   
                   if (!Config.GetProperty("EnableThroughput", true))
                   {
                       listener.ConnectionInfo.ThroughPut.Change(Timeout.Infinite, Timeout.Infinite);
                   }

                    ReflectionHelper.SetPrivateFieldValue(type, this, "_listener", listener);
                    listener.Start();

                    _rakListener = listener;
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
            _rakListener.Stop();
            var task = Task.Run(
                () =>
                {
                    try
                    {
                        OpenApi?.OnDisable();
                    }
                    finally
                    {
                        _rakListener?.Stop();
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
