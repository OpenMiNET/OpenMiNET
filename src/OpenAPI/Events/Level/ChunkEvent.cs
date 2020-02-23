using MiNET.Worlds;
using OpenAPI.World;

namespace OpenAPI.Events.Level
{
	/// <summary>
	/// 	The base class for any <see cref="ChunkColumn"/> events
	/// </summary>
	public class ChunkEvent : LevelEvent
	{
		/// <summary>
		/// 	The chunk the event occured in
		/// </summary>
		public ChunkColumn Chunk { get; }
		public ChunkEvent(ChunkColumn chunk, OpenLevel level) : base(level)
		{
			Chunk = chunk;
		}
	}
}
