using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAPI.Player;

namespace OpenAPI.Events.Player
{
    public class PlayerCreatedEvent : PlayerEvent
    {
        public PlayerCreatedEvent(OpenPlayer player) : base(player)
        {
        }
    }
}
