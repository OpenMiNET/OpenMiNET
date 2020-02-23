using System;
using MiNET.Worlds;
using OpenAPI.World;

namespace OpenAPI.Events.Level
{
	/// <summary>
	/// 	Dispatched when a chunk was loaded.
	/// </summary>
	public class ChunkLoadEvent : ChunkEvent
	{
		/// <summary>
		/// 	
		/// </summary>
		/// <param name="chunk">The chunk that was loaded in</param>
		/// <param name="level">The level the chunk was loaded in to</param>
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
