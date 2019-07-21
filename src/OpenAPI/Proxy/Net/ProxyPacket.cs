using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenAPI.Proxy.Net
{
    public abstract class ProxyPacket
    {
	    public ProxyPacket()
	    {
		    PacketId = 0x00;
	    }

	    public int PacketId { get; protected set; }

	    public virtual void Read(PacketStream stream)
	    {
	    }

	    public virtual void Write(PacketStream stream)
	    {
	    }

	    public void Read(byte[] data)
	    {
		    using (var ms = new MemoryStream(data))
		    {
			    using (var stream = new PacketStream(ms))
			    {
				    Read(stream);
			    }
		    }
	    }

	    public byte[] GetData()
	    {
		    using (var ms = new MemoryStream())
		    {
			    using (var stream = new PacketStream(ms))
			    {
				    Write(stream);
				    stream.Flush();
			    }
			    return ms.ToArray();
		    }
	    }
	}
}
