using MiNET.Utils;
using MiNET.Worlds;

namespace OpenAPI.Events.Level
{
    public class ChunkGeneratedEvent : ChunkEvent
    {
        public ChunkCoordinates Coordinates { get; }
        public ChunkGeneratedEvent(ChunkCoordinates coordinates, ChunkColumn chunk) : base(chunk)
        {
            Coordinates = coordinates;
        }
    }
}