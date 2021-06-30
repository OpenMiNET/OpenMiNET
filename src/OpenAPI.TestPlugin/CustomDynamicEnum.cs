using System.Linq;
using OpenAPI.Commands;

namespace OpenAPI.TestPlugin
{
	public class CustomDynamicEnum : CustomEnum
	{
		private string[] Values { get; } = new string[]
		{
			"option1",
			"option2",
			"option3",
			"option4"
		};
		
		public CustomDynamicEnum()
		{
			
		}
		
		/// <inheritdoc />
		public override string[] GetValues()
		{
			return Values;
		}

		/// <inheritdoc />
		public override bool SetValue(string value)
		{
			if (!Values.Contains(value))
				return false;

			Value = value;
			return true;
		}
	}
}