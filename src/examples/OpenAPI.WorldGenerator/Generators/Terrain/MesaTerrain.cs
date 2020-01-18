using OpenAPI.WorldGenerator.Generators.Effects;

namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class MesaTerrain : TerrainBase
    {
        private GroundEffect GroundEffect { get; set; }= new GroundEffect(4f);
        
        public override float GenerateNoise(OverworldGeneratorV2 rtgWorld, int x, int y, float border, float river)
        {
            return Riverized(68f + GroundEffect.Added(rtgWorld, x, y), river);
        }
    }
}