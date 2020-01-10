using MiNET.Utils;
using MiNET.Worlds;
using OpenAPI.World;

namespace OpenAPI.Events.Level
{
    public class ChunkGeneratedEvent : ChunkEvent
    {
        public ChunkCoordinates Coordinates { get; }
        public ChunkGeneratedEvent(ChunkCoordinates coordinates, ChunkColumn chunk, OpenLevel level) : base(chunk, level)
        {
            Coordinates = coordinates;
        }
    }
}