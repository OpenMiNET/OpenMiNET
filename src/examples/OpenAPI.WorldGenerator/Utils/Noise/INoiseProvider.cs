using MiNET.Utils;

namespace OpenAPI.WorldGenerator.Utils.Noise
{
    public interface INoiseProvider
    {
        NoiseMap Get(ChunkCoordinates coordinates);
    }
}