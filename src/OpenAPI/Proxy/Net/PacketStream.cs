using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using fNbt;
using log4net;
using MiNET.Net;

namespace OpenAPI.Proxy.Net
{
	public class PacketStream : System.IO.Stream
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(PacketStream));

		private CancellationToken CancelationToken { get; } = new CancellationToken(false);

		private bool IsAlive
		{
			get
			{
				if (Closed || CancelationToken.IsCancellationRequested)
				{
					return false;
				}
				return true;
			}
		}

		public bool ServerBound { get; set; } = false;
		public bool CloseBaseStream { get; set; } = false;

		/// <summary>
		/// Initializes a new stream with NetworkStream as base
		/// </summary>
		/// <param name="socket">The socket used for reading and writing</param>
		public PacketStream(Socket socket)
		{
			try
			{
				BaseStream = new NetworkStream(socket);
			}
			catch (Exception ex)
			{
				throw new PacketStreamException(ex);
			}
		}

		public PacketStream(Socket socket, CancellationToken token) : this(socket)
		{
			CancelationToken = token;
		}

		public PacketStream(Stream stream, CancellationToken token, bool closeBaseStream) : this(stream)
		{
			CancelationToken = token;
			CloseBaseStream = closeBaseStream;
		}

		/// <summary>
		/// Initializes a new stream with NetworkStream as base
		/// </summary>
		/// <param name="client">The client used for reading and writing</param>
		public PacketStream(TcpClient client) : this(client.Client) { }

		/// <summary>
		/// Initializes a new stream with NetworkStream as base
		/// </summary>
		/// <param name="client">The client used for reading and writing</param>
		public PacketStream(UdpClient client) : this(client.Client) { }

		/// <summary>
		/// Initializes a new stream based on the specified stream
		/// </summary>
		/// <param name="stream">The Stream to use</param>
		public PacketStream(System.IO.Stream stream)
		{
			try
			{
				BaseStream = stream;

				if (stream is MemoryStream)
				{
					IsMemoryBased = true;
				}
			}
			catch (Exception ex)
			{
				throw new PacketStreamException(ex);
			}
		}

		/// <summary>
		/// Initializes a new MemoryStream based stream
		/// </summary>
		public PacketStream(byte[] buffer) : this(buffer, true) { }

		/// <summary>
		/// Initializes a new MemoryStream based stream
		/// </summary>
		public PacketStream(byte[] buffer, bool writeable) : this(buffer, 0, buffer.Length, writeable) { }

		/// <summary>
		/// Initializes a new MemoryStream based stream
		/// </summary>
		public PacketStream(byte[] buffer, int index, int count) : this(buffer, index, count, true) { }

		/// <summary>
		/// Initializes a new MemoryStream based stream
		/// </summary>
		public PacketStream(byte[] buffer, int index, int count, bool writeable)
			: this(new MemoryStream(buffer, index, count, writeable)) { }

		/// <summary>
		/// Initializes a new MemoryStream based stream
		/// </summary>
		public PacketStream() : this(new MemoryStream()) { }

		protected PacketStream(byte[] socket, CancellationToken cancelationToken) : this(socket)
		{
			CancelationToken = cancelationToken;
		}

		private System.IO.Stream BaseStream { get; }
		public bool IsMemoryBased { get; } = false;

		public override int ReadByte()
		{
			try
			{
				return BaseStream.ReadByte();
			}
			catch (Exception ex)
			{
				throw new PacketStreamException(ex);
			}
		}

		public override void WriteByte(byte value)
		{
			try
			{
				BaseStream.WriteByte(value);
			}
			catch (Exception ex)
			{
				throw new PacketStreamException(ex);
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			try
			{
				return BaseStream.Read(buffer, offset, count);
			}
			catch (Exception ex)
			{
				throw new PacketStreamException(ex);
			}
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			try
			{
				BaseStream.Write(buffer, offset, count);
			}
			catch (Exception ex)
			{
				throw new PacketStreamException(ex);
			}
		}

		public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			try
			{
				return BaseStream.ReadAsync(buffer, offset, count, cancellationToken);
			}
			catch (Exception ex)
			{
				throw new PacketStreamException(ex);
			}
		}

		public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			try
			{
				return BaseStream.WriteAsync(buffer, offset, count, cancellationToken);
			}
			catch (Exception ex)
			{
				throw new PacketStreamException(ex);
			}
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			try
			{
				return BaseStream.BeginRead(buffer, offset, count, callback, state);
			}
			catch (Exception ex)
			{
				throw new PacketStreamException(ex);
			}
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
		{
			try
			{
				return BaseStream.BeginWrite(buffer, offset, offset, callback, state);
			}
			catch (Exception ex)
			{
				throw new PacketStreamException(ex);
			}
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			try
			{
				return BaseStream.EndRead(asyncResult);
			}
			catch (Exception ex)
			{
				throw new PacketStreamException(ex);
			}
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			try
			{
				BaseStream.EndWrite(asyncResult);
			}
			catch (Exception ex)
			{
				throw new PacketStreamException(ex);
			}
		}

		public void WriteBytes(byte[] data)
		{
			Write(data, 0, data.Length);
		}

		public byte[] ReadBytes(int length)
		{
			var data = new byte[length];
			var totalRead = 0;
			while (totalRead != length)
			{
				var read = Read(data, totalRead, length - totalRead);
				totalRead += read;

				if (totalRead != length)
				{
					if (!IsAlive)
					{
						throw new OperationCanceledException();
					}
					Log.WarnFormat("Read {0} bytes out of {1}", read, length);

					if ((read == 0) && (length > 0))
					{
						throw new PacketStreamException(new InvalidDataException($"No data to read. Expected {length} bytes"));
					}
				}
			}
			return data;
		}

		public void WritePrefixedBytes(byte[] data)
		{
			WriteVarInt(data.Length);
			WriteBytes(data);
		}

		public byte[] ReadPrefixedBytes(out int varIntLength)
		{
			int length = ReadVarInt(out varIntLength);
			return ReadBytes(length);
		}

		public byte[] ReadPrefixedBytes()
		{
			int length = ReadVarInt();
			return ReadBytes(length);
		}

		public void WritePacket(ProxyPacket packet)
		{
			WriteVarInt(packet.PacketId);
			WritePrefixedBytes(packet.GetData());
		}

		public ProxyPacket ReadPacket()
		{
			ProxyPacket result = null;
			int packetid = ReadVarInt();
			byte[] data = ReadPrefixedBytes();
			result = ProxyPacketFactory.CreatePacket(packetid, data, ServerBound);

			return result;
		}

		public void WritePackets(ProxyPacket[] packets)
		{
			byte[] packetData;
			using (MemoryStream ms = new MemoryStream())
			{
				using (GZipStream gzip = new GZipStream(ms,
					CompressionMode.Compress, true))
				{
					using (PacketStream packetStream = new PacketStream(gzip)
					{
						ServerBound = ServerBound
					})
					{
						for (var index = 0; index < packets.Length; index++)
						{
							var packet = packets[index];
							packetStream.WritePacket(packet);
						}
					}
				}

				packetData = ms.ToArray();
			}
			WriteVarInt(packets.Length);
			WritePrefixedBytes(packetData);
		}

		public ProxyPacket[] ReadPackets()
		{
			int packetCount = ReadVarInt();

			ProxyPacket[] packets = new ProxyPacket[packetCount];
			byte[] rawPackets = ReadPrefixedBytes();
			using (GZipStream stream = new GZipStream(new MemoryStream(rawPackets),
				CompressionMode.Decompress))
			{
				using (PacketStream ps = new PacketStream(stream)
				{
					ServerBound = ServerBound
				})
				{
					for (var i = 0; i < packets.Length; i++)
					{
						packets[i] = ps.ReadPacket();
					}
				}
			}

			return packets;
		}

		public void WriteNbtCompound(NbtCompound compound)
		{
			WriteString(compound.Name);
			WriteVarInt(compound.Count);
			foreach (var tag in compound)
			{
				WriteString(tag.Name);
				WriteUInt8((byte)tag.TagType);

				switch (tag.TagType)
				{
					case NbtTagType.Unknown:

						break;
					case NbtTagType.End:

						break;
					case NbtTagType.Byte:
						WriteByte(tag.ByteValue);
						break;
					case NbtTagType.Short:
						WriteShort(tag.ShortValue);
						break;
					case NbtTagType.Int:
						WriteVarInt(tag.IntValue);
						break;
					case NbtTagType.Long:
						WriteVarLong(tag.LongValue);
						break;
					case NbtTagType.Float:
						WriteFloat(tag.FloatValue);
						break;
					case NbtTagType.Double:
						WriteDouble(tag.DoubleValue);
						break;
					case NbtTagType.ByteArray:
						WritePrefixedBytes(tag.ByteArrayValue);
						break;
					case NbtTagType.String:
						WriteString(tag.StringValue);
						break;
					case NbtTagType.List:

						break;
					case NbtTagType.Compound:

						break;
					case NbtTagType.IntArray:
						var ints = tag.IntArrayValue;
						WriteVarInt(ints.Length);
						foreach (var val in ints)
						{
							WriteVarInt(val);
						}

						break;
					default:
						throw new ArgumentOutOfRangeException();

				}
			}
		}

		public NbtCompound ReadNbtCompound()
		{
			string compoundName = ReadString();
			NbtCompound compound;
			if (!string.IsNullOrWhiteSpace(compoundName))
			{
				compound = new NbtCompound(compoundName);
			}
			else
			{
				compound = new NbtCompound();
			}

			int tagCount = ReadVarInt();
			for (int i = 0; i < tagCount; i++)
			{
				string tagName = ReadString();
				NbtTagType tagType = (NbtTagType)ReadUInt8();
				switch (tagType)
				{
					case NbtTagType.Unknown:

						break;
					case NbtTagType.End:

						break;
					case NbtTagType.Byte:
						compound.Add(new NbtByte(tagName, ReadUInt8()));
						break;
					case NbtTagType.Short:
						compound.Add(new NbtShort(tagName, ReadShort()));
						break;
					case NbtTagType.Int:
						compound.Add(new NbtInt(tagName, ReadVarInt()));
						break;
					case NbtTagType.Long:
						compound.Add(new NbtLong(tagName, ReadVarLong()));
						break;
					case NbtTagType.Float:
						compound.Add(new NbtFloat(tagName, ReadFloat()));
						break;
					case NbtTagType.Double:
						compound.Add(new NbtDouble(tagName, ReadDouble()));
						break;
					case NbtTagType.ByteArray:
						compound.Add(new NbtByteArray(tagName, ReadPrefixedBytes()));
						break;
					case NbtTagType.String:
						compound.Add(new NbtString(tagName, ReadString()));
						break;
					case NbtTagType.List:

						break;
					case NbtTagType.Compound:

						break;
					case NbtTagType.IntArray:
						var ints = ReadVarInt();
						int[] values = new int[ints];
						for (int ind = 0; ind < values.Length; ind++)
						{
							values[ind] = ReadVarInt();
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();

				}
			}

			return compound;
		}

		public void WriteInt(int val)
		{
			byte[] data = BitConverter.GetBytes(val);
			WriteBytes(data);
		}

		public int ReadInt()
		{
			byte[] data = ReadBytes(4);
			return BitConverter.ToInt32(data, 0);
		}

		public void WriteVarInt(int value, out int bytesWritten)
		{
			int c = 0;
			var val = (uint)value;
			while (true)
			{
				if ((val & 0xFFFFFF80u) == 0)
				{
					c++;
					WriteByte((byte)val);
					break;
				}
				WriteByte((byte)((val & 0x7F) | 0x80));
				val >>= 7;
				c++;
			}

			bytesWritten = c;
		}

		public void WriteVarInt(int value)
		{
			var val = (uint)value;
			while (true)
			{
				if ((val & 0xFFFFFF80u) == 0)
				{
					WriteByte((byte)val);
					break;
				}
				WriteByte((byte)((val & 0x7F) | 0x80));
				val >>= 7;
			}
		}

		public int ReadVarInt(out int bytesRead)
		{
			uint result = 0;
			var length = 0;
			while (true)
			{
				int b = ReadByte();
				if (b == -1) break;

				var current = (byte)b;

				result |= (current & 0x7Fu) << (length++ * 7);
				if (length > 5)
				{
					throw new PacketStreamException(new InvalidDataException("VarInt may not be longer than 60 bits."));
				}
				if ((current & 0x80) != 128)
				{
					break;
				}
			}
			bytesRead = length;
			return (int)result;
		}

		public int ReadVarInt()
		{
			int read;
			return ReadVarInt(out read);
		}

		public void WriteLong(long val)
		{
			byte[] data = BitConverter.GetBytes(val);
			WriteBytes(data);
		}

		public long ReadLong()
		{
			byte[] data = ReadBytes(8);
			return BitConverter.ToInt64(data, 0);
		}

		public void WriteULong(ulong val)
		{
			byte[] data = BitConverter.GetBytes(val);
			WriteBytes(data);
		}

		public ulong ReadULong()
		{
			byte[] data = ReadBytes(8);
			return BitConverter.ToUInt64(data, 0);
		}

		public void WriteVarLong(long value)
		{
			long highBits = value;
			do
			{
				long lowBits = highBits & 0x7F;
				var b = (byte)(int)lowBits;
				highBits >>= 7;
				if (highBits > 0L)
				{
					b = (byte)(b | 0x80);
				}
				WriteByte(b);
			} while (highBits > 0L);
		}

		public long ReadVarLong(out int bytesRead)
		{
			long result = 0;
			var length = 0;
			while (true)
			{
				var current = (byte)ReadByte();
				result |= (current & 0x7Fu) << (length++ * 7);
				if (length > 9)
				{
					throw new PacketStreamException(new InvalidDataException("VarLong may not be longer than 72 bits."));
				}
				if ((current & 0x80) != 128)
				{
					break;
				}
			}
			bytesRead = length;
			return result;
		}

		public long ReadVarLong()
		{
			int read;
			return ReadVarLong(out read);
		}

		public void WriteShort(short value)
		{
			byte[] data = BitConverter.GetBytes(value);
			WriteBytes(data);
		}

		public short ReadShort()
		{
			byte[] data = ReadBytes(2);
			return BitConverter.ToInt16(data, 0);
		}

		public void WriteUShort(ushort value)
		{
			byte[] data = BitConverter.GetBytes(value);
			WriteBytes(data);
		}

		public ushort ReadUShort()
		{
			byte[] data = ReadBytes(2);
			return BitConverter.ToUInt16(data, 0);
		}

		public void WriteFloat(float value)
		{
			byte[] data = BitConverter.GetBytes(value);
			WriteBytes(data);
		}

		public float ReadFloat()
		{
			byte[] data = ReadBytes(4);
			return BitConverter.ToSingle(data, 0);
		}

		public void WriteDouble(double value)
		{
			byte[] data = BitConverter.GetBytes(value);
			WriteBytes(data);
		}

		public double ReadDouble()
		{
			byte[] data = ReadBytes(8);
			return BitConverter.ToDouble(data, 0);
		}

		public void WriteString(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				WritePrefixedBytes(new byte[0]);
				return;
			}

			byte[] data = Encoding.UTF8.GetBytes(value);
			WritePrefixedBytes(data);
		}

		public string ReadString()
		{
			byte[] data = ReadPrefixedBytes();
			if (data.Length == 0)
			{
				return string.Empty;
			}

			return Encoding.UTF8.GetString(data);
		}

		public void WriteStringArray(string[] value)
		{
			WriteVarInt(value.Length);
			foreach (string val in value)
			{
				WriteString(val);
			}
		}

		public string[] ReadStringArray()
		{
			int length = ReadVarInt();
			var array = new List<string>();
			for (var i = 0; i < length; i++)
			{
				array.Add(ReadString());
			}
			return array.ToArray();
		}

		public void WriteBool(bool value)
		{
			WriteByte((byte)(value ? 1 : 0));
		}

		public bool ReadBool()
		{
			int v = ReadByte();
			return v == 1;
		}

		public void WriteIpAddress(IPAddress address)
		{
			byte[] bytes = address.GetAddressBytes();
			WritePrefixedBytes(bytes);
		}

		public IPAddress ReadIpAddress()
		{
			byte[] bytes = ReadPrefixedBytes();
			return new IPAddress(bytes);
		}

		public void WriteIpEndPoint(IPEndPoint endpoint)
		{
			WriteBool(endpoint != null);
			if (endpoint == null) return;

			WriteIpAddress(endpoint.Address);
			WriteShort((short)endpoint.Port);
		}

		public IPEndPoint ReadIpEndPoint()
		{
			if (ReadBool())
			{
				IPAddress ip = ReadIpAddress();
				short port = ReadShort();
				return new IPEndPoint(ip, port);
			}
			else
			{
				return null;
			}
		}

		public void WriteSerializable(ISerializable value)
		{
			byte[] data;
			var formatter = new BinaryFormatter();
			using (var ms = new MemoryStream())
			{
				formatter.Serialize(ms, value);
				data = ms.ToArray();
			}
			WritePrefixedBytes(data);
		}

		public T ReadSerializable<T>() where T : ISerializable
		{
			byte[] data = ReadPrefixedBytes();
			var formatter = new BinaryFormatter();
			using (var ms = new MemoryStream(data))
			{
				return (T)formatter.Deserialize(ms);
			}
		}

		public void WriteEnum<TEnum>(TEnum value) where TEnum : struct, IConvertible
		{
			Type genericType = typeof(TEnum);
			if (!genericType.IsEnum)
			{
				throw new ArgumentException("TEnum is not an Enum.");
			}

			Type underLyingType = genericType.GetEnumUnderlyingType();
			if (typeof(int) == underLyingType)
			{
				WriteInt(Convert.ToInt32(value));
			}
			else if (typeof(byte) == underLyingType)
			{
				WriteByte(Convert.ToByte(value));
			}
			else
			{
				throw new Exception("TEnum's underlyingtype is not supported!");
			}
		}

		public void ReadEnum<TEnum>(out TEnum outValue) where TEnum : struct, IConvertible
		{
			Type genericType = typeof(TEnum);
			if (!genericType.IsEnum)
			{
				throw new ArgumentException("TEnum is not an Enum.");
			}

			Type underLyingType = genericType.GetEnumUnderlyingType();
			if (typeof(int) == underLyingType)
			{
				int value = ReadInt();
				if (Enum.TryParse(value.ToString(), out outValue))
				{
					return;
				}
			}
			else if (typeof(byte) == underLyingType)
			{
				var value = (byte)ReadByte();
				if (Enum.TryParse(value.ToString(), out outValue))
				{
					return;
				}
			}
			else
			{
				throw new Exception("TEnum's underlyingtype is not supported!");
			}
			outValue = default(TEnum);
		}

		public byte ReadUInt8()
		{
			return (byte)ReadByte();
		}

		public void WriteUInt8(byte value)
		{
			WriteByte(value);
		}

		public void Skip(int length)
		{
			ReadBytes(length);
		}

		public override void Flush()
		{
			try
			{
				BaseStream.Flush();
			}
			catch (Exception ex)
			{
				throw new PacketStreamException(ex);
			}
		}

		public override Task FlushAsync(CancellationToken cancellationToken)
		{
			try
			{
				return BaseStream.FlushAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				throw new PacketStreamException(ex);
			}
		}

		public override Task CopyToAsync(System.IO.Stream destination, int bufferSize, CancellationToken cancellationToken)
		{
			try
			{
				return BaseStream.CopyToAsync(destination, bufferSize, cancellationToken);
			}
			catch (Exception ex)
			{
				throw new PacketStreamException(ex);
			}
		}

		public override long Seek(long offset, SeekOrigin loc)
		{
			try
			{
				return BaseStream.Seek(offset, loc);
			}
			catch (Exception ex)
			{
				throw new PacketStreamException(ex);
			}
		}

		public override void SetLength(long value)
		{
			try
			{
				BaseStream.SetLength(value);
			}
			catch (Exception ex)
			{
				throw new PacketStreamException(ex);
			}
		}

		public override bool CanRead => BaseStream.CanRead;

		public override bool CanSeek => BaseStream.CanSeek;

		public override bool CanTimeout => BaseStream.CanTimeout;

		public override bool CanWrite => BaseStream.CanWrite;

		public override long Length
		{
			get
			{
				try
				{
					return BaseStream.Length;
				}
				catch (Exception ex)
				{
					throw new PacketStreamException(ex);
				}
			}
		}

		public override long Position
		{
			get
			{
				try
				{
					return BaseStream.Position;
				}
				catch (Exception ex)
				{
					throw new PacketStreamException(ex);
				}
			}
			set
			{
				try
				{
					BaseStream.Position = value;
				}
				catch (Exception ex)
				{
					throw new PacketStreamException(ex);
				}
			}
		}

		public override int ReadTimeout
		{
			get
			{
				try
				{
					return BaseStream.ReadTimeout;
				}
				catch (Exception ex)
				{
					throw new PacketStreamException(ex);
				}
			}
			set
			{
				try
				{
					BaseStream.ReadTimeout = value;
				}
				catch (Exception ex)
				{
					throw new PacketStreamException(ex);
				}
			}
		}

		public override int WriteTimeout
		{
			get
			{
				try
				{
					return BaseStream.WriteTimeout;
				}
				catch (Exception ex)
				{
					throw new PacketStreamException(ex);
				}
			}
			set
			{
				try
				{
					BaseStream.WriteTimeout = value;
				}
				catch (Exception ex)
				{
					throw new PacketStreamException(ex);
				}
			}
		}

		protected bool Closed { get; private set; } = false;

		public override void Close()
		{
			if (Closed)
			{
				return;
			}

			try
			{
				//CancelationToken.Cancel();
				if (CloseBaseStream)
					BaseStream.Close();

				Closed = true;
			}
			catch (Exception ex)
			{
				throw new PacketStreamException(ex);
			}
		}

		public byte[] ToArray()
		{
			if (!IsMemoryBased)
			{
				throw new PacketStreamException(
					new NotSupportedException("ToArray is only supported when initialized with a MemoryStream based stream"));
			}
			return ((MemoryStream)BaseStream).ToArray();
		}

		public void Clear()
		{
			if (!IsMemoryBased)
			{
				throw new PacketStreamException(
					new NotSupportedException("Clear is only supported when initialized with a MemoryStream based stream"));
			}
			BaseStream.Position = 0;
			BaseStream.SetLength(0);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!Closed)
				{
					Close();
				}

				if (CloseBaseStream)
					BaseStream.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
