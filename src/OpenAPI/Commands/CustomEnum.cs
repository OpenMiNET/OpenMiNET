using MiNET.Plugins;

namespace OpenAPI.Commands
{
	public abstract class CustomEnum
	{
		public string EnumType { get; set; } = "string";
		
		public string Value { get; protected set; }
		
		public abstract string[] GetValues();

		/// <summary>
		///		Set's the value of the parameter, should return false if not a valid option.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public abstract bool SetValue(string value);
	}
}