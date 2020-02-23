using System;
using OpenAPI.World;

namespace OpenAPI.Events.Level
{
	[Obsolete("Implementation missing")]
	public class LevelLoadEvent : LevelEvent
	{
		public LevelLoadEvent(OpenLevel world) : base(world)
		{
			throw new NotImplementedException();
		}
	}
}
