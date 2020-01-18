namespace OpenAPI.WorldGenerator.Utils.Noise.Modules
{
    public class SmoothNoiseModule : INoiseModule
    {
        public INoiseModule Primitive { get; set; }
        public float Distance { get; set; } = 12f;
        
        public SmoothNoiseModule(INoiseModule primitive)
        {
            Primitive = primitive;
        }
        
        public float GetValue(float x, float y)
        {
            float q11 = Primitive.GetValue(x - Distance, y - Distance);
            float q12 = Primitive.GetValue(x - Distance, y + Distance);

            float q21 = Primitive.GetValue(x + Distance, y - Distance);
            float q22 = Primitive.GetValue(x + Distance, y + Distance);
            
            return MathUtils.BilinearCmr(
                x, y,
                q11,
                q12,
                q21,
                q22,
                x - Distance, x + Distance, x - Distance, x + Distance);
        }

        public float GetValue(float x, float y, float z)
        {
            throw new System.NotImplementedException();
        }
    }
}