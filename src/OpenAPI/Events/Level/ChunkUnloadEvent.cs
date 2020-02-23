using System;
using MiNET.Worlds;
using OpenAPI.World;

namespace OpenAPI.Events.Level
{
	/// <summary>
	/// 	Dispatched when a <see cref="ChunkColumn"/> was unloaded
	/// </summary>
	public class ChunkUnloadEvent : ChunkEvent
	{
		/// <summary>
		/// 	
		/// </summary>
		/// <param name="chunk">The chunk that was unloaded</param>
		/// <param name="level">The level the chunk was unloaded from</param>
		public ChunkUnloadEvent(ChunkColumn chunk, OpenLevel level) : base(chunk, level)
		{
			
		}
	}
}
