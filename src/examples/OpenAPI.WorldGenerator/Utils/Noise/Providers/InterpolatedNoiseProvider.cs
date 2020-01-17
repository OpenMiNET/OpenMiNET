using MiNET.Utils;

namespace OpenAPI.WorldGenerator.Utils.Noise.Providers
{
    public class InterpolatedNoiseProvider : INoiseProvider
    {
        public INoiseModule Module { get; }
        public InterpolatedNoiseProvider(INoiseModule module2D)
        {
            Module = module2D;
        }
        
        public NoiseMap Get(ChunkCoordinates coordinates)
        {
            int minX = ((coordinates.X) * 16) ;
            int minZ = ((coordinates.Z) * 16) ;
            var maxX = ((coordinates.X + 1) << 4) ;
            var maxZ = ((coordinates.Z + 1) << 4) ;
            
            int cx = (coordinates.X * 16);
            int cz = (coordinates.Z * 16);

            float q11 = Module.GetValue(minX, minZ);
            float q12 = Module.GetValue(minX, maxZ);

            float q21 = Module.GetValue(maxX, minZ);
            float q22 = Module.GetValue(maxX, maxZ);
            
            NoiseMap noiseMap = new NoiseMap(new float[16 * 16]);
            for (int x = 0; x < 16; x++)
            {
                float rx = cx + x;

                for (int z = 0; z < 16; z++)
                {
                    float rz = cz + z;
                    
                    noiseMap.Set(x,z, MathUtils.BilinearCmr(
                        rx, rz,
                        q11,
                        q12,
                        q21,
                        q22,
                        minX, maxX, minZ, maxZ));
                }
            }

            return noiseMap;
        }
    }
}