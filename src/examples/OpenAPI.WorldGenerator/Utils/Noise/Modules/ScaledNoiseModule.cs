using OpenAPI.WorldGenerator.Utils.Noise.Attributes;

namespace OpenAPI.WorldGenerator.Utils.Noise.Modules
{
    public class ScaledNoiseModule : FilterNoise, INoiseModule
    {
        public ScaledNoiseModule(INoiseModule noiseModule)
        {
            base.Primitive = noiseModule;
        }
        
        [Modifier]
        public float ScaleX { get; set; }
        
        [Modifier]
        public float ScaleY { get; set; }
        
        [Modifier]
        public float ScaleZ { get; set; }
        
        
        public float GetValue(float x, float y)
        {
            return Primitive.GetValue(x * ScaleX, y * ScaleZ);
        }

        public float GetValue(float x, float y, float z)
        {
            return Primitive.GetValue(x * ScaleX, y * ScaleY, z * ScaleZ);
        }
    }
}