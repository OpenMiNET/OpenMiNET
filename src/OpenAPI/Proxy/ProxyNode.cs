using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using log4net;
using MiNET.Utils;

namespace OpenAPI.Proxy
{
	public class ProxyNode : BaseClient
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(ProxyNode));

		private RSACryptoServiceProvider RSA { get; set; }
		public ProxyNode(TcpClient client, DedicatedThreadPool packetThreadPool, bool isServer) : base(client, packetThreadPool, isServer)
		{
		}

		private void GenerateOrLoadCertificates()
		{
			if (!LoadPrivateKey())
			{
				Log.Warn($"Could not find privatekey.xml file!");
				Environment.Exit(1);
			}
		}

		private bool LoadPrivateKey()
		{
			const string privateKeyFileName = "privatekey.xml";
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
	}
}
