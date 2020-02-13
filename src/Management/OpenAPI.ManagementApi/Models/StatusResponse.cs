namespace OpenAPI.ManagementApi.Models
{
    public class StatusResponse
    {
        public string Id { get; set; }
        public int Players { get; set; }
        public int MaxPlayers { get; set; }
        public string Motd { get; set; }
        
        public string Version { get; set; }
        public int Protocol { get; set; }

        public StatusResponse()
        {
            
        }
    }
}