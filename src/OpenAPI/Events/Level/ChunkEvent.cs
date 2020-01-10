using MiNET.Worlds;
using OpenAPI.World;

namespace OpenAPI.Events.Level
{
	public class ChunkEvent : LevelEvent
	{
		public ChunkColumn Chunk { get; }
		public ChunkEvent(ChunkColumn chunk, OpenLevel level) : base(level)
		{
			Chunk = chunk;
		}
	}
}
