using OpenAPI.WorldGenerator.Generators.Effects;

namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class PlainsTerrain : TerrainBase
    {
        private GroundEffect _groundEffect = new GroundEffect(4f);
        
        public override float GenerateNoise(OverworldGeneratorV2 world, int x, int y, float border, float river)
        {
            return Riverized(65f + _groundEffect.Added(world, x, y), river);
            return TerrainPlains(x, y, world, river, 160f, 10f, 60f, 200f, 66f);
        }
    }
}