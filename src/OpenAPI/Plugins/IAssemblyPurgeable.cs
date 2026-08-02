using System.Reflection;

namespace OpenAPI.Plugins
{
	/// <summary>
	///		Implemented by anything that can hold a reference into a plugin assembly and must
	///		release it when that plugin is unloaded.
	/// </summary>
	/// <remarks>
	///		Implementations register themselves with <see cref="OpenApi.RegisterPurgeable"/> and
	///		are driven from <see cref="OpenPluginManager.PurgeAssembly"/>.
	///
	///		Self-registration rather than the host walking a collection: a level's dispatcher and
	///		tick scheduler must be reachable for teardown whether or not that level was ever
	///		registered with the <see cref="World.OpenLevelManager"/>.
	///
	///		A single missed reference silently prevents the assembly from ever unloading, so new
	///		registries that can hold plugin instances, <see cref="System.Type"/>s,
	///		<see cref="MethodInfo"/>s or delegates should implement this rather than inventing
	///		their own teardown path.
	/// </remarks>
	public interface IAssemblyPurgeable
	{
		/// <summary>
		///		Releases every reference this object holds into <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly">The plugin assembly being unloaded.</param>
		/// <returns>How many references were released, for logging.</returns>
		int PurgeAssembly(Assembly assembly);
	}
}
