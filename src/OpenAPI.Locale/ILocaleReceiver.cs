using MiNET;

namespace OpenAPI.Locale
{
    public interface ILocaleReceiver : ILocalizable
    {
        void SendLocalizedTitle(string text, TitleType type = TitleType.Title, int fadeIn = 6, int fadeOut = 6,
            int stayTime = 20, Player sender = null);

        void SendLocalizedTitle(string text, object[] parameters = null, TitleType type = TitleType.Title, int fadeIn = 6, int fadeOut = 6,
            int stayTime = 20, Player sender = null);

        void SendLocalizedMessage(string text, MessageType type = MessageType.Chat, MiNET.Player sender = null);
        void SendLocalizedMessage(string text, object[] parameters = null, MessageType type = MessageType.Chat, MiNET.Player sender = null);
    }
}
