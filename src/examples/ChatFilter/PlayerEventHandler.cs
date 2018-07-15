using OpenAPI.Events;
using OpenAPI.Events.Player;

namespace ChatFilter
{
    public class PlayerEventHandler : IEventHandler
    {
		private OpenAPI.OpenAPI Api { get; }
		public PlayerEventHandler(OpenAPI.OpenAPI api)
		{
			Api = api;
		}

		[EventHandler]
	    public void OnPlayerJoin(PlayerJoinEvent e)
	    {
			ChatCompanion companion = new ChatCompanion(e.Player);
			e.Player.SetAttribute(companion);
	    }

	    [EventHandler]
	    public void OnPlayerQuit(PlayerQuitEvent e)
	    {
		    e.Player.SetAttribute<ChatCompanion>(null);
	    }

		[EventHandler(EventPriority.Highest)]
	    public void OnPlayerChat(PlayerChatEvent e)
		{
			if (e.IsCancelled) return;

			var companion = e.Player.GetAttribute<ChatCompanion>();
			if (companion == null) return;

			if (companion.Muted)
			{
				e.SetCancelled(true);
				e.Player.SendMessage($"§4You have been muted, you may not send any chat messages!");
				return;
			}

			if (!companion.CanChat())
			{
				e.SetCancelled(true);
				e.Player.SendMessage($"§4Calm down, no need to spam the chat!");
				return;
			}

			string filtered = e.Message;

			if (!companion.Filter(filtered, out filtered))
			{
				e.SetCancelled(true);
				e.Player.SendMessage($"§4Please watch your language!");
				return;
			}

			e.Message = filtered;

			companion.SentChatMesssage();
		}

	    public void CleanUp()
	    {
			
	    }
    }
}
