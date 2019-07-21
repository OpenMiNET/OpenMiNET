using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using log4net;
using MiNET.Utils;

namespace OpenAPI.Proxy
{
    public class ProxyServer
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(ProxyServer));

		private object _startLock = new object();
		public bool Started { get; private set; } = false;

		private OpenServer Server { get; }
		private TcpListener Listener { get; }
		private ConcurrentDictionary<EndPoint, ServerClient> Clients { get; }

		public RSACryptoServiceProvider RSA { get; private set; }
		private DedicatedThreadPool PacketThreadPool { get; }
		public bool EnableEncryption { get; set; } = true;
		public ProxyServer(OpenServer server, IPEndPoint endPoint)
	    {
		    Server = server;

		    PacketThreadPool = new DedicatedThreadPool(new DedicatedThreadPoolSettings(Environment.ProcessorCount, ThreadType.Foreground, "PacketThreadPool"));
			Listener = TcpListener.Create(endPoint.Port);
			Clients = new ConcurrentDictionary<EndPoint, ServerClient>();

		    GenerateOrLoadCertificates();
	    }

		private void GenerateOrLoadCertificates()
		{
			if (!LoadPublicKey())
			{
				Log.Info($"Generating keypair...");

				CspParameters param = new CspParameters(1);
				RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(param);

				string privateKey = rsa.ToXmlString(true);
				File.WriteAllText("privatekey.xml", privateKey);
				Log.Info("Private key saved to file.");

				string publicKey = rsa.ToXmlString(false);
				File.WriteAllText("publickey.xml", publicKey);
				Log.Info("Public key saved to file!");

				rsa.Dispose();

				LoadPublicKey();
			}
		}

		private bool LoadPublicKey()
		{
			const string privateKeyFileName = "publickey.xml";
			if (File.Exists(privateKeyFileName))
			{
				try
				{
					string privateKeyXml = File.ReadAllText(privateKeyFileName);

					RSA = new RSACryptoServiceProvider();
					RSA.FromXmlString(privateKeyXml);
					return true;
				}
				catch
				{
					Log.Error($"Failed to load public key file!");
					return false;
				}
			}
			else
			{
				Log.Error($"Could not find the public key!");
				return false;
			}
		}

		public void Start()
		{
			lock (_startLock)
			{
				Listener.Start();
				Started = true;

				Listener.BeginAcceptTcpClient(OnClientConnect, null);
			}
		}

		private void OnClientConnect(IAsyncResult ar)
		{
			try
			{
				TcpClient client = Listener.EndAcceptTcpClient(ar);
				ServerClient c = new ServerClient(this, client, PacketThreadPool);
				c.OnConnectionLost += OnConnectionLost;
				c.CollectTraficData = false;

				if (Clients.TryAdd(client.Client.RemoteEndPoint, c))
				{
					Log.Info("New connection from: " + client.Client.RemoteEndPoint);
					if (c.Init(0))
					{
						Log.Info("Connection initiated.");
					}
					else
					{
						Log.Warn("Failed to initiate connection...");
					}
				}
			}
			catch (ObjectDisposedException ex)
			{
				if (Started)
					Log.Error("Uh uh!", ex);
			}
			finally
			{
				if (Started)
				{
					Listener.BeginAcceptTcpClient(OnClientConnect, null);
				}
			}
		}

		private void OnConnectionLost(object sender, EventArgs eventArgs)
		{
			if (sender is ServerClient client)
			{
				Clients.TryRemove(client.RemoteEndPoint, out ServerClient trash);
			}
		}

		public void Stop()
		{
			lock (_startLock)
			{
				if (!Started) return;

				Started = false;
				Listener.Stop();
			}
		}
	}
}
