using MiNET.Worlds;
using OpenAPI.Player;

namespace OpenAPI.Events.Player
{
	/// <summary>
	///		Dispatched when an <see cref="OpenPlayer"/>'s gamemode changed
	/// </summary>
	public class PlayerGamemodeChangeEvent : PlayerEvent
	{
		/// <summary>
		///		The players gamemode before the change
		/// </summary>
		public GameMode OldGameMode { get; }
		
		/// <summary>
		///		The players gamemode after the change
		/// </summary>
		public GameMode NewGameMode { get; }
		
		/// <summary>
		///		The trigger that caused the players gamemode to change
		/// </summary>
		public PlayerGamemodeChangeTrigger Trigger { get; }
		
		/// <inheritdoc />
		public PlayerGamemodeChangeEvent(OpenPlayer player, GameMode oldGameMode, GameMode newGameMode, PlayerGamemodeChangeTrigger trigger) : base(player)
		{
			OldGameMode = oldGameMode;
			NewGameMode = newGameMode;
			Trigger = trigger;
		}

		public enum PlayerGamemodeChangeTrigger
		{
			Self,
			Other
		}
	}
}