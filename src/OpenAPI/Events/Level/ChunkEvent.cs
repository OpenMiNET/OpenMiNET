using MiNET.Worlds;
using OpenAPI.World;

namespace OpenAPI.Events.Level
{
	public class ChunkEvent : Event
	{
		public ChunkColumn Chunk { get; }
		public ChunkEvent(ChunkColumn chunk) : base()
		{
			Chunk = chunk;
		}
	}
}
