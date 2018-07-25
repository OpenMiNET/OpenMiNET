using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using log4net;
using MiNET.Utils;
using OpenAPI.Proxy.Net;
using OpenAPI.Proxy.Net.Clientbound;
using OpenAPI.Proxy.Net.Serverbound;
using OpenAPI.Utils;

namespace OpenAPI.Proxy
{
    public class ServerClient : BaseClient
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(ServerClient));
		private FastRandom Rnd { get; }
		private byte[] Verification { get; set; }
		private ManualResetEventSlim HandshakeResetEvent { get; }
		private bool ConnectionInitiated { get; set; } = false;

		private ProxyServer Host { get; }
		private Aes Rijndael { get; }
		public ServerClient(ProxyServer host, TcpClient client, DedicatedThreadPool packetThreadPool) : base(client, packetThreadPool, false)
		{
			Host = host;

			Rnd = new FastRandom();

			Verification = new byte[128];
			Rnd.NextBytes(Verification);

			Rijndael = new AesManaged();
			Rijndael.Mode = CipherMode.CBC;
			Rijndael.Padding = PaddingMode.PKCS7;
			Rijndael.KeySize = 256;
			Rijndael.BlockSize = 128;

			Rijndael.GenerateKey();
			Rijndael.GenerateIV();

			RegisterHandlers();
			HandshakeResetEvent = new ManualResetEventSlim(false);
		}

		private void RegisterHandlers()
		{
			PacketHandlers.Register<HandshakeResponse>(HandleHandshakeResponse);
			PacketHandlers.Register<HandshakeConfirmation>(HandleHandshakeConfirmation);


			RegisterPacketHandlers(PacketHandlers);
		}

		protected virtual void RegisterPacketHandlers(TypeToActionMap<ProxyPacket> map) { }

		private void HandleHandshakeConfirmation(HandshakeConfirmation packet)
		{
			byte[] keyPart = Rijndael.Key.Take(8).ToArray();
			byte[] ivPart = Rijndael.IV.Take(8).ToArray();

			byte[] joined;
			using (MemoryStream ms = new MemoryStream())
			{
				ms.Write(keyPart, 0, keyPart.Length);
				ms.Write(ivPart, 0, ivPart.Length);
				joined = ms.ToArray();
			}

			SHA512 sha = SHA512.Create();
			var hashed = sha.ComputeHash(joined);
			sha.Dispose();

			if (Host.RSA.VerifyHash(hashed, "SHA512", packet.Confirmation))
			{
				if (Host.EnableEncryption)
				{
					EnableEncryption(Rijndael.Key, Rijndael.IV);
					Log.Info("Handshake confirmed, encryption enabled!");
				}
				else
				{
					Log.Info("Handshake confirmed, encryption disabled!");
				}
			}
			else
			{
				Log.Warn("Failed to confirm handshake!");
			}
		}

		private void HandleHandshakeResponse(HandshakeResponse handshakeResponse)
		{
			bool failed = false;
			try
			{
				SHA512 sha = SHA512.Create();
				var hashed = sha.ComputeHash(Verification);
				sha.Dispose();

				if (!Host.RSA.VerifyHash(hashed, "SHA512", handshakeResponse.SignedVerification))
				{
					failed = true;
				}
			}
			catch (Exception ex)
			{
				Log.Error("Error while handling handshake response!", ex);
				failed = true;
			}
			finally
			{
				if (failed)
				{
					SendPacket(new HandshakeStatus()
					{
						Status = 0,
						SecretIV = new byte[0],
						SecretKey = new byte[0]
					});

					Log.Info("Invalid verification token!");
					Client.Close();

					ConnectionInitiated = false;
				}
				else
				{
					SendPacket(new HandshakeStatus()
					{
						Status = 1,
						SecretKey = Host.RSA.Encrypt(Rijndael.Key, true),
						SecretIV = Host.RSA.Encrypt(Rijndael.IV, true)
					});

					Log.Info("Verified secret!");
					ConnectionInitiated = true;

				//	ConnectionTypeInstance?.RegisterPacketHandlers(PacketHandlers);
				}

				HandshakeResetEvent?.Set();
			}
		}

		public bool Init(byte connectionType)
		{
			try
			{
				BeginRead();

				SendPacket(new Handshake()
				{
					Verification = Verification,
					Encryption = Host.EnableEncryption,
					ConnectionType = connectionType
				});

				HandshakeResetEvent.Wait(GetCancellationToken);
				return ConnectionInitiated;
			}
			catch (Exception exception)
			{
				Log.Error("Failed to initiate", exception);
				return false;
			}
		}

		public EventHandler OnConnectionLost;
		protected override void ConnectionLost()
		{
			OnConnectionLost?.Invoke(this, EventArgs.Empty);
			//ConnectionTypeInstance?.ConnectionLost();
		}
	}
}
