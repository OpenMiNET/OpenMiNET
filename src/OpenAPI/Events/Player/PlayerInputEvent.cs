using OpenAPI.Player;
using OpenAPI.Utils;

namespace OpenAPI.Events.Player
{
    /// <summary>
    ///		Raised when a tracked key changes state, for players with
    ///		<see cref="OpenPlayer.CapturePlayerInputMode"/> enabled.
    /// </summary>
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
