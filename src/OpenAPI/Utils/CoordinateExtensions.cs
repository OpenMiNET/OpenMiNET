using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiNET.Utils;
using MiNET.Utils.Vectors;

namespace OpenAPI.Utils
{
    public static class CoordinateExtensions
    {
        public static BlockCoordinates ToBlockCoordinates(this PlayerLocation location)
        {
            return new BlockCoordinates(
                (int) Math.Min(Math.Floor(location.X), Math.Ceiling(location.X)),
                (int)Math.Min(Math.Floor(location.Y), Math.Ceiling(location.Y)),
                (int)Math.Min(Math.Floor(location.Z), Math.Ceiling(location.Z))
            );
        }
    }
}
