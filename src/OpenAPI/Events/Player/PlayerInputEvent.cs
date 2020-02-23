using System;
using OpenAPI.Player;
using OpenAPI.Utils;

namespace OpenAPI.Events.Player
{
    [Obsolete("Not currrently implemented")]
    public class PlayerInputEvent : PlayerEvent
    {
        public PlayerInput Input { get; }
        public PlayerInputState State { get; }
        public PlayerInputEvent(OpenPlayer player, PlayerInput input, PlayerInputState state) : base(player)
        {
            Input = input;
            State = state;
        }
    }
}
