using OpenAPI.Player;

namespace OpenAPI.Events.Player
{
	/// <summary>
	/// 	Dispatched whenever an <see cref="OpenPlayer"/> says something in chat
	/// </summary>
	public class PlayerChatEvent : PlayerEvent
	{
		private string _original;

		/// <summary>
		/// Gets or sets the format.
		/// </summary>
		/// <value>
		/// The format.
		/// </value>
		public string Format { get; set; }

		/// <summary>
		/// Gets or sets the format parameters.
		/// </summary>
		/// <value>
		/// The format parameters.
		/// </value>
		public string[] FormatParameters { get; set; }

		/// <summary>
		/// 	The message sent by the player
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// Gets or sets the recipients.
		/// </summary>
		/// <value>
		/// The recipients.
		/// </value>
		public OpenPlayer[] Recipients { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PlayerChatEvent" /> class.
		/// </summary>
		/// <param name="player">The player.</param>
		/// <param name="message">The message.</param>
		/// <param name="format">The format.</param>
		/// <param name="formatParameters">The format parameters.</param>
		/// <param name="recipients">The recipients.</param>
		public PlayerChatEvent(
			OpenPlayer player,
			string message,
			string format = "<{0}> {1}",
			string[] formatParameters = null,
			OpenPlayer[] recipients = null)
			: base(player)
		{
			_original = message;
			Format = format;
			FormatParameters = formatParameters ?? new string[] { player.DisplayName ?? player.Username, message };
			Message = message;
			Recipients = recipients;
		}
	}
}
