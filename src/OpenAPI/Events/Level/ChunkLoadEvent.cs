using System;
using MiNET.Worlds;
using OpenAPI.World;

namespace OpenAPI.Events.Level
{
	public class ChunkLoadEvent : ChunkEvent
	{
		public ChunkLoadEvent(ChunkColumn chunk) : base(chunk)
		{
			
		}
	}

	public class ChunkSaveEvent : ChunkEvent
	{
		public ChunkSaveEvent(ChunkColumn chunk) : base(chunk)
		{
			
		}
	}
}
