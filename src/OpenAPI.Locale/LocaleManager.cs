using System;
using System.Collections.Concurrent;
using System.Reflection;
using OpenAPI.Locale.Providers;

namespace OpenAPI.Locale
{
    public static class LocaleManager
    {
        private static ConcurrentDictionary<Assembly, AssemblyLocaleSettings> AssemblySettings { get; } =
            new ConcurrentDictionary<Assembly, AssemblyLocaleSettings>();

        public static Func<Assembly, AssemblyLocaleSettings> DefaultsProvider { get; set; } = (assembly) =>
        {
            return new AssemblyLocaleSettings(assembly);
        };

        public static AssemblyLocaleSettings Configure(Assembly assembly, Action<AssemblyLocaleSettings> configurator)
        {
            return AssemblySettings.AddOrUpdate(assembly, AddValueFactory, (assembly1, settings) =>
            {
                configurator?.Invoke(settings);
                
                return settings;
            });
        }

        private static AssemblyLocaleSettings AddValueFactory(Assembly arg)
        {
            return DefaultsProvider.Invoke(arg);
        }

        public static ILocaleProvider GetLocaleProvider(Assembly assembly)
        {
            if (AssemblySettings.TryGetValue(assembly, out var localeSettings))
            {
                return localeSettings.LocaleProvider;
            }
            
            return Configure(assembly, null).LocaleProvider;
        }
    }

    public class AssemblyLocaleSettings
    {
        public ILocaleProvider LocaleProvider { get; set; }

        public AssemblyLocaleSettings(Assembly assembly)
        {
            LocaleProvider = new ResourceLocaleProvider(assembly);
        }
    }
}