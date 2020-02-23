using OpenAPI.Player;

namespace OpenAPI.GameEngine
{
    public static class PlayerExtensions
    {
        public static string GetIdentifier(this OpenPlayer player)
        {
            if (player.IsXbox)
                return player.CertificateData.ExtraData.Xuid;

            return player.ClientUuid.ToString();
        }
    }
}