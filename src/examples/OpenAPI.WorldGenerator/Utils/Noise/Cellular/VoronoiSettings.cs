namespace OpenAPI.WorldGenerator.Utils.Noise.Cellular
{
    public class VoronoiSettings
    {
        public double ShortestDistance { get; set; } = 32000000.0;
        public double NextDistance { get; set; } = 32000000.0;
        public double ClosestX { get; set; } = 32000000.0;
        public double ClosestZ { get; set; } = 32000000.0;

        public VoronoiSettings() : this(32000000.0)
        {
            
        }

        public VoronoiSettings(double defaultValues)
        {
            ShortestDistance = defaultValues;
            NextDistance = defaultValues;
            ClosestX = defaultValues;
            ClosestZ = defaultValues;
        }
    }
}