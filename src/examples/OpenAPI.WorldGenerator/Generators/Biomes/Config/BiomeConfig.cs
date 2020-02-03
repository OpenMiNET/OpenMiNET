namespace OpenAPI.WorldGenerator.Generators.Biomes.Config
{
    public class BiomeConfig
    {
        public bool SurfaceBlendIn { get; set; } = true;
        public bool SurfaceBlendOut { get; set; } = true;
        
        public bool AllowRivers { get; set; } = true;
        public bool AllowScenicLakes { get; set; } = true;
        
        public BiomeConfig()
        {
            
        }
    }
}