
//
// WARNING: T4 GENERATED CODE - DO NOT EDIT
// Please edit "Protocol.xml" instead
// 

// ReSharper disable InconsistentNaming
using OpenAPI.Utils;
using System.Net;
using System;
using fNbt;

namespace OpenAPI.Proxy.Net
{
	public class ProxyPacketFactory
	{
		public static ProxyPacket CreatePacket(int packetId, byte[] buffer, bool serverBound)
		{
			ProxyPacket packet; 
			if (serverBound)
			{
					switch (packetId)
					{
						case 0x00:
								packet = new Serverbound.HandshakeResponse();
								packet.Read(buffer);
								return packet;
						case 0x01:
								packet = new Serverbound.HandshakeConfirmation();
								packet.Read(buffer);
								return packet;
					}
			}
			else
			{
				switch (packetId)
				{
						case 0x00:
								packet = new Clientbound.Handshake();
								packet.Read(buffer);
								return packet;
						case 0x01:
								packet = new Clientbound.HandshakeStatus();
								packet.Read(buffer);
								return packet;
				}
			}
			return null;
		}
	}
}

namespace OpenAPI.Proxy.Net.Clientbound
{
	public class Handshake : ProxyPacket
	{
		public byte[] Verification;
		public bool Encryption;
		public byte ConnectionType;
		public Handshake()
		{
			PacketId = 0x00;
		}

		public override void Write(PacketStream stream)
		{
			stream.WritePrefixedBytes(Verification);
			stream.WriteBool(Encryption);
			stream.WriteUInt8(ConnectionType);
		}

		public override void Read(PacketStream stream)
		{
			Verification = stream.ReadPrefixedBytes();
			Encryption = stream.ReadBool();
			ConnectionType = stream.ReadUInt8();
		}
	}
}

namespace OpenAPI.Proxy.Net.Serverbound
{
	public class HandshakeResponse : ProxyPacket
	{
		public byte[] SignedVerification;
		public HandshakeResponse()
		{
			PacketId = 0x00;
		}

		public override void Write(PacketStream stream)
		{
			stream.WritePrefixedBytes(SignedVerification);
		}

		public override void Read(PacketStream stream)
		{
			SignedVerification = stream.ReadPrefixedBytes();
		}
	}
}

namespace OpenAPI.Proxy.Net.Clientbound
{
	public class HandshakeStatus : ProxyPacket
	{
		public byte Status;
		public byte[] SecretKey;
		public byte[] SecretIV;
		public HandshakeStatus()
		{
			PacketId = 0x01;
		}

		public override void Write(PacketStream stream)
		{
			stream.WriteUInt8(Status);
			stream.WritePrefixedBytes(SecretKey);
			stream.WritePrefixedBytes(SecretIV);
		}

		public override void Read(PacketStream stream)
		{
			Status = stream.ReadUInt8();
			SecretKey = stream.ReadPrefixedBytes();
			SecretIV = stream.ReadPrefixedBytes();
		}
	}
}

namespace OpenAPI.Proxy.Net.Serverbound
{
	public class HandshakeConfirmation : ProxyPacket
	{
		public byte[] Confirmation;
		public HandshakeConfirmation()
		{
			PacketId = 0x01;
		}

		public override void Write(PacketStream stream)
		{
			stream.WritePrefixedBytes(Confirmation);
		}

		public override void Read(PacketStream stream)
		{
			Confirmation = stream.ReadPrefixedBytes();
		}
	}
}


