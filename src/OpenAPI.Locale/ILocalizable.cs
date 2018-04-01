using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAPI.Locale
{
    public interface ILocalizable
    {
        CultureInfo Culture { get; }
    }
}
