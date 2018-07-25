using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAPI.Proxy.Net
{
	public class PacketStreamException : Exception
	{
		public PacketStreamException(Exception innerException) : base("", innerException) { }
	}
}
