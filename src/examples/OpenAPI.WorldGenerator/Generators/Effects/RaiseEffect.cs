namespace OpenAPI.WorldGenerator.Generators.Effects
{
    /*
    * // just adds a constant increase
    *
    * @author Zeno410
    */
    public class RaiseEffect : HeightEffect
    {

        // just adds a number
        public readonly float Height;

        public RaiseEffect(float height)
        {
            this.Height = height;
        }

        public override float Added(OverworldGeneratorV2 generator, float x, float y)
        {

            return Height;
        }
    }
}