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
using MiNET.Plugins;
using MiNET.Utils;
using OpenAPI.Events.Server;
using OpenAPI.Utils;

namespace OpenAPI
{
    public class OpenServer : MiNET.MiNetServer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(OpenServer));

        private OpenApi OpenApi { get; set; }
        private Thread ProcessingThread { get; set; }

        public static DedicatedThreadPool FastThreadPool => ReflectionHelper.GetPrivateStaticPropertyValue<DedicatedThreadPool>(typeof(MiNetServer), "FastThreadPool");
        public OpenServer()
        {
            OpenApi = new OpenApi();
        }

        public new bool StartServer()
        {
            var type = typeof(MiNetServer);
            
            UdpClient c = ReflectionHelper.GetPrivateFieldValue<UdpClient>(type, this, "_listener");
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
                    openInfo = new OpenServerInfo(OpenApi,
                        ReflectionHelper.GetPrivateFieldValue<ConcurrentDictionary<IPEndPoint, PlayerNetworkSession>>(
                            type, this, "_playerSessions"), OpenApi.LevelManager);
                    ServerInfo = openInfo;
                    openInfo.Init();

                    OpenApi.ServerInfo = openInfo;

                    global::MiNET.Items.ItemFactory.CustomItemFactory = OpenApi.ItemFactory;

                    GreylistManager = GreylistManager ?? new GreylistManager(this);
                    SessionManager = SessionManager ?? new SessionManager();
                    LevelManager = OpenApi.LevelManager;
                    PlayerFactory = OpenApi.PlayerManager;
                }

                MotdProvider = OpenApi.MotdProvider;

                OpenApi.OnEnable(this);

                if (ServerRole == ServerRole.Full || ServerRole == ServerRole.Proxy)
                {
                    var listener = new UdpClient(Endpoint);
                    if (!System.Runtime.InteropServices.RuntimeInformation
                        .IsOSPlatform(OSPlatform.Windows))
                    {
                        listener.Client.ReceiveBufferSize = 1024 * 1024 * 3;
                        listener.Client.SendBufferSize = 4096;
                    }
                    else
                    {
                        listener.Client.ReceiveBufferSize = int.MaxValue;
                        listener.Client.SendBufferSize = int.MaxValue;
                        listener.DontFragment = false;
                        listener.EnableBroadcast = false;

                        uint IOC_IN = 0x80000000;
                        uint IOC_VENDOR = 0x18000000;
                        uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                        listener.Client.IOControl((int) SIO_UDP_CONNRESET, new byte[] {Convert.ToByte(false)}, null);
                    }

                    ReflectionHelper.SetPrivateFieldValue(type, this, "_listener", listener);

                    var processData = type.GetMethod("ProcessDatagrams",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    
                    ProcessingThread = new Thread((l) =>
                    {
                        processData?.Invoke(this, new object[] {(UdpClient) l});
                    }) {IsBackground = true};
                    
                    ProcessingThread.Start(listener);
                }

                openInfo?.OnEnable();

                ReflectionHelper.SetPrivateFieldValue(type, this, "_tickerHighPrecisionTimer",
                    new HighPrecisionTimer(10, async (o) => {
                        //ReflectionHelper.InvokePrivateMethod(this, "SendTick", new[] {o});
                        var sessions =
                            ReflectionHelper
                                .GetPrivateFieldValue<ConcurrentDictionary<IPEndPoint, PlayerNetworkSession>>(
                                    type, this, "_playerSessions");

                        var tasks = sessions.Values.Select(session => session.SendTickAsync());
                        await Task.WhenAll(tasks);
                    }, true, true));

                Log.Info("Server open for business on port " + Endpoint?.Port + " ...");

                OpenApi.EventDispatcher.DispatchEvent(new ServerReadyEvent());
                
                return true;
            }
            catch (Exception e)
            {
                Log.Error("Error during startup!", e);
                StopServer();
            }

            return false;
        }

        public new bool StopServer()
	    {
		    if (base.StopServer())
		    {
				OpenApi?.OnDisable();
			    return true;
		    }

		    return false;
	    }
    }
}
