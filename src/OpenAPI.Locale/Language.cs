using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using OpenAPI.Locale.Providers;

namespace OpenAPI.Locale
{
	public static class Language
	{
		public static readonly Regex FindLanguageRegex = new Regex("{L:([A-Za-z0-9_.]*)}", RegexOptions.Compiled);

	    public static ResourceManager Fallback = null;


	    public static string GetLocalizedMessage(this ILocalizable target, string message, params object[] paramaters)
	    {
		    var localeProvider = LocaleManager.GetLocaleProvider(Assembly.GetCallingAssembly());
		    
		    message = FindLanguageRegex.Replace(message,
			    match => GetLanguageString(target, localeProvider, match.Groups[1].Value, paramaters));
		    
		    return message;
	    }

	    public static string GetLocalizedMessage(this ILocalizable target, ILocaleProvider rm, string message, params object[] paramaters)
	    {
	        message = FindLanguageRegex.Replace(message, match => GetLanguageString(target, rm, match.Groups[1].Value, paramaters));
	        return message;
	    }

        private static string GetLanguageString(ILocalizable target, ILocaleProvider rm, string languageEntry, params object[] args)
		{
			string str = rm?.GetString(languageEntry, target.Culture) ??  "{{L:" + languageEntry + "}}";
			if (args == null) args = new object[0];

			var newArgs = new object[args.Length];
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i] == null)
				{
					newArgs[i] = "";
				}
				else
				{
					newArgs[i] = GetLocalizedMessage(target, rm, args[i].ToString());
				}
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
