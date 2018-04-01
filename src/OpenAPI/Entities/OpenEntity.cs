using MiNET.Entities;
using OpenAPI.World;

namespace OpenAPI.Entities
{
	public class OpenEntity : Entity
	{
		public OpenLevel OpenLevel { get; }
		public OpenEntity(int entityTypeId, OpenLevel level) : base(entityTypeId, level)
		{
			OpenLevel = level;
		}
	}
}
