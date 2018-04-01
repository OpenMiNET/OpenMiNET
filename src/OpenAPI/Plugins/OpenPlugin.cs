using System;
using System.Reflection;
using log4net;
using MiNET.Plugins.Attributes;
using OpenAPI.Events.Player;

namespace OpenAPI.Plugins
{
    public abstract class OpenPlugin
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(OpenPlugin));

        public OpenPluginInfo Info { get; internal set; }

        protected OpenPlugin()
        {
            Info = LoadPluginInfo();
        //    Log.Info(JsonConvert.SerializeObject(Info, Formatting.Indented));
        }

        public abstract void Enabled(OpenAPI api);
        public abstract void Disabled(OpenAPI api);

        #region OpenPlugin Initialisation

	    private OpenPluginInfo LoadPluginInfo()
	    {
		    var type = GetType();

		    //var info = new OpenPluginInfo();
		    var info = type.GetCustomAttribute<OpenPluginInfo>();
		    if (info == null) info = new OpenPluginInfo();

		    // Fill info from the plugin's type/assembly
		    var assembly = type.Assembly;

		    if (string.IsNullOrWhiteSpace(info.Name))
			    info.Name = type.FullName;

		    if (string.IsNullOrWhiteSpace(info.Version) && !string.IsNullOrEmpty(assembly.Location))
			    info.Version = AssemblyName.GetAssemblyName(assembly.Location)?.Version?.ToString() ?? "";

				//info.Version = assembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version ?? "";

		    if (string.IsNullOrWhiteSpace(info.Description))
			    info.Description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "";

		    if (string.IsNullOrWhiteSpace(info.Author))
			    info.Author = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "";

		    return info;
	    }

	    #endregion
    }
}
