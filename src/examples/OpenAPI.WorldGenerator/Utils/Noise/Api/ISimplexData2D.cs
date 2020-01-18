namespace OpenAPI.WorldGenerator.Utils.Noise.Api
{
    public interface ISimplexData2D {

        void AddToDeltaX(double val);

        void AddToDeltaY(double val);

        double GetDeltaX();

        void SetDeltaX(double deltaX);

        double GetDeltaY();

        void SetDeltaY(double deltaY);

        void Clear();

        IDataRequest Request();
        
        public interface IDataRequest {

            void Apply(double attn, double extrapolation, double gx, double gy, int giSph2, double dx, double dy);
        }
    }
}