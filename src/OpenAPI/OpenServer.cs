using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using log4net;
using MiNET;
using MiNET.Items;
using MiNET.Plugins;
using MiNET.Utils;
using MiNET.Worlds;

namespace OpenAPI
{
    public class OpenServer : MiNET.MiNetServer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(OpenServer));

        private TcpListener _tcpListener { get; set; }

        private OpenAPI OpenApi { get; set; }
        public OpenServer()
        {
            OpenApi = new OpenAPI();
        }

        public new bool StartServer()
        {
            UdpClient c = GetPrivateFieldValue<UdpClient>(typeof(MiNetServer), this, "_listener");
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
                        SetPrivatePropertyValue(typeof(MiNetServer), this, "Endpoint", new IPEndPoint(ip, port));
                    }
                }

                ServerManager = ServerManager ?? new DefaultServerManager(this);
                OpenServerInfo openInfo = null;

                if (ServerRole == ServerRole.Full || ServerRole == ServerRole.Node)
                {
                    PluginManager = new PluginManager();
                    openInfo = new OpenServerInfo(OpenApi, GetPrivateFieldValue<ConcurrentDictionary<IPEndPoint, PlayerNetworkSession>>(typeof(MiNetServer), this, "_playerSessions"), OpenApi.LevelManager);
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

                if (ServerRole == ServerRole.Node)
                {
                    _tcpListener = new TcpListener(Endpoint);
                    _tcpListener.BeginAcceptSocket(SocketConnected, _tcpListener);
                }

                if (ServerRole == ServerRole.Full || ServerRole == ServerRole.Proxy)
                {
                    var listener = new UdpClient(Endpoint);
                    if (IsRunningOnMono())
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
                        listener.Client.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
                    }

                    SetPrivateFieldValue(typeof(MiNetServer), this, "_listener", listener);

                    new Thread((l) =>
                    {
                        var processData = typeof(MiNetServer).GetMethod("ProcessDatagrams",
                            BindingFlags.NonPublic | BindingFlags.Instance);

                        processData?.Invoke(this, new object[] { (UdpClient)l });
                    }) { IsBackground = true }.Start(listener);
                }

                openInfo?.OnEnable();

	            var a = typeof(MiNetServer).GetMethod("SendTick",
		            BindingFlags.NonPublic | BindingFlags.Instance);

				SetPrivateFieldValue(typeof(MiNetServer), this, "_tickerHighPrecisionTimer", new HighPrecisionTimer(10, (o) => a.Invoke(this, new object[]{o}), true, true));

				Log.Info("Server open for business on port " + Endpoint?.Port + " ...");

                return true;
            }
            catch (Exception e)
            {
                Log.Error("Error during startup!", e);
                StopServer();
            }

            return false;
        }

        private void SocketConnected(IAsyncResult ar)
        {
            Socket client = _tcpListener.EndAcceptSocket(ar);
            _tcpListener.BeginAcceptSocket(SocketConnected, _tcpListener);

        }

        private void Callback(IAsyncResult ar)
        {
          
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

      //  protected override void OnServerShutdown()
     //   {
      //      OpenApi?.OnDisable();
      //  }

        /*     private string _currentPath;
        private Assembly MyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            Assembly result;

            AssemblyName name = new AssemblyName(args.Name);

            if (TryLoadAssembly(Path.Combine(_currentPath, name.Name + ".dll"), out result))
            {
                return result;
            }

            if (TryLoadAssembly(Path.Combine(_currentPath, name.Name + ".exe"), out result))
            {
                return result;
            }

            if (TryLoadAssembly(args.Name + ".dll", out result))
            {
                return result;
            }

            return null;
        }

        private bool TryLoadAssembly(string file, out Assembly result)
        {
            try
            {
                if (File.Exists(file))
                {
                    result = Assembly.LoadFile(file);
                    return true;
                }
            }
            catch
            {
                result = null;
                return false;
            }

            result = null;
            return false;
        }*/

        /// <summary>
        /// Returns a _private_ Property Value from a given Object. Uses Reflection.
        /// Throws a ArgumentOutOfRangeException if the Property is not found.
        /// </summary>
        /// <typeparam name="T">Type of the Property</typeparam>
        /// <param name="obj">Object from where the Property Value is returned</param>
        /// <param name="propName">Propertyname as string.</param>
        /// <returns>PropertyValue</returns>
        private static T GetPrivatePropertyValue<T>(Type t, object obj, string propName)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            PropertyInfo pi = t.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (pi == null) throw new ArgumentOutOfRangeException("propName", string.Format("Property {0} was not found in Type {1}", propName, obj.GetType().FullName));
            return (T)pi.GetValue(obj, null);
        }

        /// <summary>
        /// Returns a private Property Value from a given Object. Uses Reflection.
        /// Throws a ArgumentOutOfRangeException if the Property is not found.
        /// </summary>
        /// <typeparam name="T">Type of the Property</typeparam>
        /// <param name="obj">Object from where the Property Value is returned</param>
        /// <param name="propName">Propertyname as string.</param>
        /// <returns>PropertyValue</returns>
        private static T GetPrivateFieldValue<T>(Type t, object obj, string propName)
        {
            if (obj == null) throw new ArgumentNullException("obj");
           // Type t = obj.GetType();
            FieldInfo fi = null;
            while (fi == null && t != null)
            {
                fi = t.GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                t = t.BaseType;
            }
            if (fi == null) throw new ArgumentOutOfRangeException("propName", string.Format("Field {0} was not found in Type {1}", propName, obj.GetType().FullName));
            return (T)fi.GetValue(obj);
        }

        /// <summary>
        /// Sets a _private_ Property Value from a given Object. Uses Reflection.
        /// Throws a ArgumentOutOfRangeException if the Property is not found.
        /// </summary>
        /// <typeparam name="T">Type of the Property</typeparam>
        /// <param name="obj">Object from where the Property Value is set</param>
        /// <param name="propName">Propertyname as string.</param>
        /// <param name="val">Value to set.</param>
        /// <returns>PropertyValue</returns>
        private static void SetPrivatePropertyValue<T>(Type t, object obj, string propName, T val)
        {
           // Type t = obj.GetType();
            if (t.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) == null)
                throw new ArgumentOutOfRangeException("propName", string.Format("Property {0} was not found in Type {1}", propName, obj.GetType().FullName));
            t.InvokeMember(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance, null, obj, new object[] { val });
        }

        /// <summary>
        /// Set a private Property Value on a given Object. Uses Reflection.
        /// </summary>
        /// <typeparam name="T">Type of the Property</typeparam>
        /// <param name="obj">Object from where the Property Value is returned</param>
        /// <param name="propName">Propertyname as string.</param>
        /// <param name="val">the value to set</param>
        /// <exception cref="ArgumentOutOfRangeException">if the Property is not found</exception>
        private static void SetPrivateFieldValue<T>(Type t, object obj, string propName, T val)
        {
            if (obj == null) throw new ArgumentNullException("obj");
           // Type t = obj.GetType();
            FieldInfo fi = null;
            while (fi == null && t != null)
            {
                fi = t.GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                t = t.BaseType;
            }
            if (fi == null) throw new ArgumentOutOfRangeException("propName", string.Format("Field {0} was not found in Type {1}", propName, obj.GetType().FullName));
            fi.SetValue(obj, val);
        }
    }
}
