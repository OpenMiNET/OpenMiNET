using System;
using System.Reflection;
using log4net;
using MiNET.Plugins.Attributes;
using OpenAPI.Events.Player;

namespace OpenAPI.Plugins
{
	/// <summary>
	/// 	Provides the base class for any plugin.
	/// 	All plugins running on OpenAPI must have atleast one class inhereting this.
	/// </summary>
    public abstract class OpenPlugin
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(OpenPlugin));

        public OpenPluginInfo Info { get; internal set; }

        protected OpenPlugin()
        {
            Info = LoadPluginInfo();
        //    Log.Info(JsonConvert.SerializeObject(Info, Formatting.Indented));
        }

        /// <summary>
        /// 	The method that gets invoked as soon as a plugin gets Enabled.
        /// 	Any initialization should be done in here.
        /// </summary>
        /// <param name="api">An instance to OpenApi</param>
        public abstract void Enabled(OpenApi api);
        
        /// <summary>
        /// 	The method that gets invoked as soon as a plugin gets Disabled.
        /// 	Any content initialized in <see cref="Enabled"/> should be de-initialized in here.
        /// </summary>
        /// <param name="api">An instance to OpenApi</param>
        public abstract void Disabled(OpenApi api);

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
