using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAPI.Player;

namespace OpenAPI.Events.Player
{
    /// <summary>
    ///     Dispatched whenever a <see cref="OpenPlayer"/> was created by the <see cref="OpenPlayerManager"/>
    /// </summary>
    public class PlayerCreatedEvent : PlayerEvent
    {
        /// <summary>
        ///     
        /// </summary>
        /// <param name="player">The player that got created</param>
        public PlayerCreatedEvent(OpenPlayer player) : base(player)
        {
        }
    }
}
