using LibNoise;
using LibNoise.Transformer;

namespace OpenAPI.WorldGenerator.Utils.Noise
{
    public class BetterTurbulence : Turbulence, IModule2D
    {
        public float GetValue(float x, float y)
        {
            float x1 = x + 0.1894226f;
            float y1 = y + 0.9937134f;
            float x2 = x + 0.4046478f;
            float y2 = y + 0.2766113f;

            return ((IModule2D) this._sourceModule).GetValue(
                x + ((IModule2D) this._xDistortModule).GetValue(x1, y1) * this._power,
                y + ((IModule2D) this._yDistortModule).GetValue(x2, y2) * this._power);
        }
    }
}