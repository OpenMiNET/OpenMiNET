using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace OpenAPI.Plugins
{
    public class PluginContext : AssemblyLoadContext
	{
		readonly string testAssemblyDirectory;

		public PluginContext(string testAssemblyDirectory)
		{
			this.testAssemblyDirectory = testAssemblyDirectory;
		}

		protected override Assembly Load(AssemblyName assemblyName)
		{
			return LoadFromFolderOrDefault(assemblyName);
		}

		Assembly LoadFromFolderOrDefault(AssemblyName assemblyName)
		{
			try
			{
				var path = Path.Combine(testAssemblyDirectory, assemblyName.Name);

				if (File.Exists(path + ".dll"))
					return LoadFromAssemblyPath(path + ".dll");

				if (File.Exists(path + ".exe"))
					return LoadFromAssemblyPath(path + ".exe");

				//TODO: Probably missing something here. What if it's
				//             a transitive nuget dependency, not literally in the
				//             test project's build output folder?

				return null;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}

			return null;
		}
	}
}
