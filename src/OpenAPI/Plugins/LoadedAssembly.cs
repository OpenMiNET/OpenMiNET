using System;
using System.Collections.Generic;
using System.Reflection;

namespace OpenAPI.Plugins
{
	public class LoadedAssembly
	{
		public Assembly Assembly { get; }
		public List<Type> PluginTypes { get; }

		/// <summary>
		///		The plugin assemblies this one binds to — the plugins its constructors ask for.
		/// </summary>
		/// <remarks>
		///		Informational: this is what <see cref="LoadedPlugin.Dependencies"/> reports.
		///		Unloading does <em>not</em> follow these — see <see cref="Dependents"/>.
		/// </remarks>
		public List<Assembly> Dependencies { get; }

		/// <summary>
		///		The plugin assemblies that bind to this one. The reverse of
		///		<see cref="Dependencies"/>.
		/// </summary>
		/// <remarks>
		///		This is the direction unloading follows. A dependent holds this assembly's types
		///		and instances, so this assembly cannot be released until every dependent has been
		///		unloaded first — which is why the reload unit is a plugin plus its transitive
		///		dependents rather than a single plugin.
		///
		///		Spelled out because the two directions are easy to confuse, and getting it
		///		backwards produces a cascade that unloads the wrong set.
		/// </remarks>
		public List<Assembly> Dependents { get; }

		public string Path { get; }

		public LoadedAssembly(
			Assembly assembly,
			IEnumerable<Type> pluginInstances,
			IEnumerable<Assembly> dependencies,
			IEnumerable<Assembly> dependents,
			string path)
		{
			Assembly = assembly;
			PluginTypes = new List<Type>(pluginInstances);
			Dependencies = new List<Assembly>(dependencies);
			Dependents = new List<Assembly>(dependents);
			Path = path;
		}
	}
}
