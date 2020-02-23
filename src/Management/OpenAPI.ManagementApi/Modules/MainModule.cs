using MiNET.Net;
using Nancy;
using OpenAPI.ManagementApi.Models;

namespace OpenAPI.ManagementApi.Modules
{
    public sealed class MainModule : NancyModule
    {
        private OpenApi Api { get; }
        public MainModule(OpenApi api)
        {
            Api = api;
            
            Get("/", Index);
        }

        private object Index(dynamic args)
        {
            return Response.AsJson(new StatusResponse()
            {
                Version = McpeProtocolInfo.GameVersion,
                Protocol = McpeProtocolInfo.ProtocolVersion,
                
                Motd = Api.MotdProvider.Motd,
                Players = Api.MotdProvider.NumberOfPlayers,
                MaxPlayers = Api.MotdProvider.MaxNumberOfPlayers,
                Id = Api.MotdProvider.ServerId.ToString()
            });
        }
    }
}