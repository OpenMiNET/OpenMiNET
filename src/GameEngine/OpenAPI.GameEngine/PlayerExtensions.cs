using OpenAPI.GameEngine.Games;
using OpenAPI.GameEngine.Games.Teams;
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

        public static PlayerGameAttribute GetGameAttribute(this OpenPlayer player)
        {
            return player.GetAttribute<PlayerGameAttribute>();
        }

        internal static void SetTeam(this OpenPlayer player, Team team)
        {
            player.GetGameAttribute().Team = team;
        }

        internal static void SetGame(this OpenPlayer player, Game game)
        {
            player.GetGameAttribute().JoinGame(game);
        }
    }
}