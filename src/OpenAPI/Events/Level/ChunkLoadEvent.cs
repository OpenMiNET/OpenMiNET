using System;
using MiNET.Worlds;
using OpenAPI.World;

namespace OpenAPI.Events.Level
{
	public class ChunkLoadEvent : ChunkEvent
	{
		public ChunkLoadEvent(ChunkColumn chunk, OpenLevel level) : base(chunk, level)
		{
			
		}
	}

	public class ChunkSaveEvent : ChunkEvent
	{
		public ChunkSaveEvent(ChunkColumn chunk, OpenLevel level) : base(chunk, level)
		{
			
		}
	}
}
