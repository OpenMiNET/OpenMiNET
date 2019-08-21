using System.Globalization;

namespace OpenAPI.Locale.Providers
{
    public interface ILocaleProvider
    {
        string GetString(string languageEntry, CultureInfo targetCulture);
    }
}