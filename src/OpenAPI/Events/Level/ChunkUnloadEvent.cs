using System;
using MiNET.Worlds;
using OpenAPI.World;

namespace OpenAPI.Events.Level
{
	public class ChunkUnloadEvent : ChunkEvent
	{
		public ChunkUnloadEvent(ChunkColumn chunk) : base(chunk)
		{
			
		}
	}
}
