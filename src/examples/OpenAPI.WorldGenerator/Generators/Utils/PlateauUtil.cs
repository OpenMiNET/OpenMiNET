namespace OpenAPI.WorldGenerator.Generators.Utils
{
    public class PlateauUtil
    {
        public static float StepIncrease(float simplexVal, float start, float finish, float height) {
            return (simplexVal <= start) ? 0 : (simplexVal >= finish) ? height : ((simplexVal - start) / (finish - start)) * height;
        }
    }
}