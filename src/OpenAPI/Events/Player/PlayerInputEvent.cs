using OpenAPI.Player;
using OpenAPI.Utils;

namespace OpenAPI.Events.Player
{
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
