using System;
using System.Net;
using MiNET;
using MiNET.Net;
using MiNET.Net.RakNet;

namespace OpenAPI
{
	public class OpenMotdProvider : MotdProvider
	{
		public OpenApi Api { get; }
		public OpenMotdProvider(OpenApi api)
		{
			Api = api;
			Motd = Environment.MachineName;

			MaxNumberOfPlayers = 100;
			NumberOfPlayers = 0;
		}

		public override string GetMotd(ConnectionInfo serverInfo, IPEndPoint caller, bool eduMotd = false)
		{
			var protocolVersion = McpeProtocolInfo.ProtocolVersion;
			var clientVersion = McpeProtocolInfo.GameVersion;
			var edition = "MCPE";

			return string.Format($"{edition};{Motd};{protocolVersion};{clientVersion};{NumberOfPlayers};{MaxNumberOfPlayers};{Motd.GetHashCode() + caller.Address.Address + caller.Port};{SecondLine};Survival;");
		}
	}
}
