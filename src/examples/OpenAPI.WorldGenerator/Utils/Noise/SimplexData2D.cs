using MiNET.Utils;
using OpenAPI.WorldGenerator.Utils.Noise.Api;
using OpenAPI.WorldGenerator.Utils.Noise.Primitives;

namespace OpenAPI.WorldGenerator.Utils.Noise
{
    public abstract class SimplexData2D : ISimplexData2D
    {

        private double deltaX;
        private double deltaY;

        private SimplexData2D()
        {
            this.Clear();
        }

        /**
     * Gets a new {@link SimplexData2D.Disk} multi-evaluation data object for use in generating jitter effects.
     *
     * @return a new instance of SimplexData2D.Disk
     * @since 1.0.0
     */
        public static ISimplexData2D NewDisk()
        {
            return new Disk();
        }

        /**
     * Gets a new {@link SimplexData2D.Derivative} multi-evaluation data object for use in generating jitter effects.
     *
     * @return a new instance of SimplexData2D.Derivative
     * @since 1.0.0
     */
        public static ISimplexData2D NewDerivative()
        {
            return new Derivative();
        }

        public double GetDeltaX()
        {
            return this.deltaX;
        }

        public void SetDeltaX(double deltaX)
        {
            this.deltaX = deltaX;
        }

        public double GetDeltaY()
        {
            return this.deltaY;
        }

        public void SetDeltaY(double deltaY)
        {
            this.deltaY = deltaY;
        }

        public void AddToDeltaX(double val)
        {
            this.deltaX += val;
        }

        public void AddToDeltaY(double val)
        {
            this.deltaY += val;
        }

        public void Clear()
        {
            this.SetDeltaX(0.0d);
            this.SetDeltaY(0.0d);
        }

        public ISimplexData2D.IDataRequest Request()
        {
            return new Disk.DiskDataRequest(this);
        }

        public class Disk : SimplexData2D
        {

            public Disk() : base()
            {

            }

            public ISimplexData2D.IDataRequest request()
            {
                return new DiskDataRequest(this);
            }

            public class DiskDataRequest : ISimplexData2D.IDataRequest
            {
                private SimplexData2D Data { get; }

                public DiskDataRequest(SimplexData2D data2D)
                {
                    Data = data2D;
                }

                public void Apply(double attn, double extrapolation, double gx, double gy, int gi_sph2, double dx,
                    double dy)
                {
                    double attnSq = attn * attn;
                    double extrap = attnSq * attnSq * extrapolation;
                    Data.AddToDeltaX(extrap * SimplexPerlin.GradientsSph2[gi_sph2]);
                    Data.AddToDeltaY(extrap * SimplexPerlin.GradientsSph2[gi_sph2 + 1]);
                }
            }
        }

        public class Derivative : SimplexData2D
        {

            public Derivative()
            {

            }

            public ISimplexData2D.IDataRequest request()
            {
                return new DerivativeDataRequest(this);
            }

            public class DerivativeDataRequest : ISimplexData2D.IDataRequest
            {
                private SimplexData2D Data { get; }

                public DerivativeDataRequest(SimplexData2D data2D)
                {
                    Data = data2D;
                }

                public void Apply(double attn, double extrapolation, double gx, double gy, int gi_sph2, double dx,
                    double dy)
                {
                    double attnSq = attn * attn;
                    Data.AddToDeltaX((gx * attn - 8 * dx * extrapolation) * attnSq * attn);
                    Data.AddToDeltaY((gy * attn - 8 * dy * extrapolation) * attnSq * attn);
                }
            }
        }
    }
}