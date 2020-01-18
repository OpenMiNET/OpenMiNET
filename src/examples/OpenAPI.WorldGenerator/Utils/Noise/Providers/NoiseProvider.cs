using MiNET.Utils;

namespace OpenAPI.WorldGenerator.Utils.Noise.Providers
{
    public class NoiseProvider : INoiseProvider
    {
        private INoiseModule Module { get; }
        public NoiseProvider(INoiseModule module)
        {
            Module = module;
        }
        
        public NoiseMap Get(ChunkCoordinates coordinates)
        {
            int cx = (coordinates.X * 16);
            int cz = (coordinates.Z * 16);
            
            NoiseMap noiseMap = new NoiseMap(new float[16 * 16]);
            for (int x = 0; x < 16; x++)
            {
                float rx = cx + x;

                for (int z = 0; z < 16; z++)
                {
                    float rz = cz + z;
                    noiseMap.Set(x, z, Module.GetValue(rx, rz));
                }
            }

            return noiseMap;
        }
    }
}