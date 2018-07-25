using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using log4net;
using MiNET.Net;
using MiNET.Utils;
using OpenAPI.Proxy.Net;
using OpenAPI.Utils;

namespace OpenAPI.Proxy
{
	public class BaseClient
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(BaseClient));

		public EndPoint RemoteEndPoint => Client.Client.RemoteEndPoint;

		protected TcpClient Client { get; }
		public TypeToActionMap<ProxyPacket> PacketHandlers { get; }
		private bool IsServer { get; }

		private Thread ReadingThread;
		private CancellationTokenSource CancellationToken { get; }
		protected CancellationToken GetCancellationToken => CancellationToken.Token;

		public bool IsConnected { get; set; } = false;
		private bool EncryptionEnabled { get; set; } = false;
		private CryptoStream CryptoRead { get; set; }
		private CryptoStream CryptoWrite { get; set; }
		private Aes Rijndael { get; set; }
		protected ManualResetEventSlim EncryptionResetEvent { get; }

		private DedicatedThreadPool PacketThreadPool { get; }
		public BaseClient(TcpClient client, DedicatedThreadPool packetThreadPool, bool isServer)
		{
			PacketThreadPool = packetThreadPool;
			IsConnected = client.Connected;
			Client = client;
			IsServer = isServer;
			PacketHandlers = new TypeToActionMap<ProxyPacket>();
			CancellationToken = new CancellationTokenSource();

			EncryptionResetEvent = new ManualResetEventSlim(true);
		}

		public bool CollectTraficData { get; set; } = true;
		private long _packetsSent = 0;
		private long _packetsReceived = 0;
		private long _bytesSent = 0;
		private long _bytesReceived = 0;

		public void GetTrafficInfo(out long packetSent, out long packetsReceived, out long bytesSent, out long bytesReceived)
		{
			long sentPackets = Interlocked.Exchange(ref _packetsSent, 0);
			long receivedPackets = Interlocked.Exchange(ref _packetsReceived, 0);

			long sentBytes = Interlocked.Exchange(ref _bytesSent, 0);
			long readBytes = Interlocked.Exchange(ref _bytesReceived, 0);

			packetSent = sentPackets;
			packetsReceived = receivedPackets;
			bytesSent = sentBytes;
			bytesReceived = readBytes;
		}

		protected void BeginRead()
		{
			ReadingThread = new Thread(() =>
			{
				while (!CancellationToken.IsCancellationRequested)
				{
					if (Client.Client != null && Client.Connected)
					{
						ProxyPacket packet = ReadPacket();
						if (packet != null)
						{
							PacketThreadPool.QueueUserWorkItem(() =>
							{
								Stopwatch sw = Stopwatch.StartNew();
								if (!PacketHandlers.TryInvokeAction(packet))
								{
									Log.Warn("Unhandled packet: 0x" + packet.PacketId.ToString("X2") + " " + packet.ToString());
								}
								sw.Stop();
								if (sw.ElapsedMilliseconds > 250)
								{
									Log.Warn($"Packet handling for 0x{packet.PacketId.ToString("X2")} took {sw.ElapsedMilliseconds}ms!");
								}
							});
						}
						else
						{
							if (!Client.Connected)
							{
								Log.Info("Connection lost.");
								break;
							}
							Log.Warn("Packet was null!");
						}
					}
					else
					{
						Log.Info("Connection lost.");
						IsConnected = false;
						CancellationToken.Cancel();

						ConnectionLost();
					}
				}
			});
			ReadingThread.Start();
		}

		public void SendPacket(ProxyPacket packet)
		{
			try
			{
				Stream stream;
				if (EncryptionEnabled)
				{
					stream = CryptoWrite;
				}
				else
				{
					stream = new NetworkStream(Client.Client);
				}

				int bytesWritten = 0;
				byte[] packetData = packet.GetData();
				using (PacketStream ps = new PacketStream(stream, GetCancellationToken, false)
				{
					ServerBound = !IsServer
				})
				{
					ps.WriteVarInt(packet.PacketId, out bytesWritten);
					ps.WritePrefixedBytes(packetData);

					ps.Flush();
				}

				bytesWritten += packetData.Length;

				if (CollectTraficData)
				{
					Interlocked.Increment(ref _packetsSent);
					Interlocked.Add(ref _bytesSent, bytesWritten);
				}
			}
			catch (Exception ex)
			{
				Log.Error("Could not write packet", ex);
				IsConnected = false;
				CancellationToken.Cancel();

				ConnectionLost();
			}
		}

		private ProxyPacket ReadPacket()
		{
			try
			{
				Stream stream;
				if (EncryptionEnabled)
				{
					stream = CryptoRead;
				}
				else
				{
					stream = new NetworkStream(Client.Client);
				}

				int bytesRead = 0;
				ProxyPacket result;
				using (PacketStream ps = new PacketStream(stream, CancellationToken.Token, false)
				{
					ServerBound = !IsServer
				})
				{
					int packetId = ps.ReadVarInt(out bytesRead);
					byte[] packetData = ps.ReadPrefixedBytes(out bytesRead);
					result = ProxyPacketFactory.CreatePacket(packetId, packetData, !IsServer);
					bytesRead += packetData.Length;
				}

				if (CollectTraficData)
				{
					Interlocked.Increment(ref _packetsReceived);
					Interlocked.Add(ref _bytesReceived, bytesRead);
				}

				return result;
			}
			catch (Exception ex)
			{
				Log.Error("Could not read packet!", ex);
				IsConnected = false;
				CancellationToken.Cancel();
				return null;
			}
		}

		protected void EnableEncryption(byte[] key, byte[] iv)
		{
			var keyv = new Rfc2898DeriveBytes(key, iv, 32768);

			Rijndael = new AesManaged();
			Rijndael.Mode = CipherMode.CBC;
			Rijndael.Padding = PaddingMode.PKCS7;
			Rijndael.KeySize = 256;
			Rijndael.BlockSize = 128;

			Rijndael.Key = keyv.GetBytes(Rijndael.KeySize / 8);
			Rijndael.IV = keyv.GetBytes(Rijndael.BlockSize / 8);

			CryptoWrite = new CryptoStream(new NetworkStream(Client.Client), Rijndael.CreateEncryptor(),
				CryptoStreamMode.Write);

			CryptoRead = new CryptoStream(new NetworkStream(Client.Client), Rijndael.CreateDecryptor(),
				CryptoStreamMode.Read);

			EncryptionEnabled = true;
		}

		protected virtual void ConnectionLost()
		{

		}
	}
}
