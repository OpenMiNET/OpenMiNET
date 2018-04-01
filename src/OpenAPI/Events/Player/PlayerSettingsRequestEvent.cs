using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiNET.Net;
using OpenAPI.Player;

namespace OpenAPI.Events.Player
{
	public class PlayerSettingsRequestEvent : PlayerEvent
	{
		public PlayerSettingsRequestEvent(OpenPlayer player, McpeServerSettingsRequest request) : base(player)
		{
		}
	}
}
