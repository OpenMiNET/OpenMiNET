using OpenAPI.Player;

namespace OpenAPI.ManagementApi.Models
{
    public class PlayerInfo
    {
        public string Username { get; set; }

        public PlayerInfo(OpenPlayer player)
        {
           // XUID = player.CertificateData.ExtraData.Xuid;
            Username = player.CertificateData.ExtraData.DisplayName;
            
        }
    }
}