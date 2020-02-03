namespace OpenAPI.WorldGenerator.Generators.Effects
{
    /**
 * @author Zeno410
 */
    public abstract class HeightEffect {

        public abstract float Added(OverworldGeneratorV2 generator, float x, float y);

        public HeightEffect Plus(HeightEffect added) {

            return new SummedHeightEffect(this, added);
        }
    }

    public class SummedHeightEffect : HeightEffect
    {

        private HeightEffect _one;
        private HeightEffect _two;

        public SummedHeightEffect(HeightEffect one, HeightEffect two)
        {

            this._one = one;
            this._two = two;
        }

        public override float Added(OverworldGeneratorV2 generator, float x, float y)
        {
            return _one.Added(generator, x, y) + _two.Added(generator, x, y);
        }
    }

}