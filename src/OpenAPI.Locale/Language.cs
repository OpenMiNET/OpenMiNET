using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;

namespace OpenAPI.Locale
{
	public static class Language
	{
		public static readonly Regex FindLanguageRegex = new Regex("{L:([A-Za-z0-9_]*)}", RegexOptions.Compiled);

	    public static ResourceManager Fallback = null;


        public static string GetLocalizedMessage(this ILocalizable target, string message, params object[] paramaters)
		{
		    var rm = GetResourceManager(Assembly.GetCallingAssembly());
			message = FindLanguageRegex.Replace(message, match => GetLanguageString(target, rm, match.Groups[1].Value, paramaters));
			return message;
		}

	    public static string GetLocalizedMessage(this ILocalizable target, ResourceManager rm, string message, params object[] paramaters)
	    {
	        message = FindLanguageRegex.Replace(message, match => GetLanguageString(target, rm, match.Groups[1].Value, paramaters));
	        return message;
	    }

        private static string GetLanguageString(ILocalizable target, ResourceManager rm, string languageEntry, params object[] args)
		{
			string str = rm?.GetString(languageEntry, target.Culture) ??  "{{L:" + languageEntry + "}}";

			var newArgs = new object[args.Length];
			for (int i = 0; i < args.Length; i++)
			{
				newArgs[i] = GetLocalizedMessage(target, rm, args[i].ToString());
			}
			return string.Format(str, newArgs);
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

	        return Fallback;
	    }
	}
}
