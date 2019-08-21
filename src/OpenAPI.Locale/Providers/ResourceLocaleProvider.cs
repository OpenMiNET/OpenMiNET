using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace OpenAPI.Locale.Providers
{
    public class ResourceLocaleProvider : ILocaleProvider
    {
        public static ResourceManager Default = null;

        private ResourceManager ResourceManager { get; }
        public ResourceLocaleProvider() : this(Assembly.GetCallingAssembly())
        {
            
        }

        public ResourceLocaleProvider(Assembly assembly)
        {
            ResourceManager = GetResourceManager(assembly);
        }
        
        public string GetString(string languageEntry, CultureInfo culture)
        {
            return ResourceManager.GetString(languageEntry, culture);
        }
        
        public static ResourceManager GetResourceManager(Assembly assembly)
        {
            string[] resourceNames = assembly.GetManifestResourceNames();

            var resourceName = resourceNames.FirstOrDefault(x => x.Contains("Language"));
            if (!string.IsNullOrWhiteSpace(resourceName))
            {
                string baseName = Path.GetFileNameWithoutExtension(resourceName);
                ResourceManager resourceManager = new ResourceManager(baseName, assembly);

                return resourceManager;
            }

            return Default;
        }
    }
}