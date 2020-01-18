namespace OpenAPI.WorldGenerator.Utils.Noise.Modules
{
    public class PipeNoiseModule : FilterNoise, INoiseModule
    {
        #region Ctor/Dtor

        #endregion

        #region IModule1D Members

        /*
        /// <summary>
        /// Generates an output value given the coordinates of the specified input value.
        /// </summary>
        /// <param name="x">The input coordinate on the x-axis.</param>
        /// <returns>The resulting output value.</returns>
        public float GetValue(float x)
        {
            x *= _frequency;

            return _source1D.GetValue(x);
        }*/

        #endregion

        #region IModule2D Members

        /// <summary>
        /// Generates an output value given the coordinates of the specified input value.
        /// </summary>
        /// <param name="x">The input coordinate on the x-axis.</param>
        /// <param name="y">The input coordinate on the y-axis.</param>
        /// <returns>The resulting output value.</returns>
        public float GetValue(float x, float y)
        {
            x *= _frequency;
            y *= _frequency;

            return _source.GetValue(x, y);
        }

        #endregion

        #region IModule3D Members

        /// <summary>
        /// Generates an output value given the coordinates of the specified input value.
        /// </summary>
        /// <param name="x">The input coordinate on the x-axis.</param>
        /// <param name="y">The input coordinate on the y-axis.</param>
        /// <param name="z">The input coordinate on the z-axis.</param>
        /// <returns>The resulting output value.</returns>
        public float GetValue(float x, float y, float z)
        {
            x *= _frequency;
            y *= _frequency;
            z *= _frequency;

            return _source.GetValue(x, y, z);
        }

        #endregion

        #region IModule4D Members

/*
        /// <summary>
        /// Generates an output value given the coordinates of the specified input value.
        /// </summary>
        /// <param name="x">The input coordinate on the x-axis.</param>
        /// <param name="y">The input coordinate on the y-axis.</param>
        /// <param name="z">The input coordinate on the z-axis.</param>
        /// <param name="t">The input coordinate on the t-axis.</param>
        /// <returns>The resulting output value.</returns>
        public float GetValue(float x, float y, float z, float t)
        {
            x *= _frequency;
            y *= _frequency;
            z *= _frequency;
            t *= _frequency;

            return _source4D.GetValue(x, y, z, t);
        }*/

        #endregion
    }
}