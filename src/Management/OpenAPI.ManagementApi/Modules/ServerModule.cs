using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using Nancy;
using OpenAPI.ManagementApi.Models;
using OpenAPI.ManagementApi.Models.Levels;
using OpenAPI.ManagementApi.Models.Plugins;
using OpenAPI.ManagementApi.Utils;
using OpenAPI.Player;

namespace OpenAPI.ManagementApi.Modules
{
    public sealed class ServerModule : NancyModule
    {
        private OpenApi Api { get; }
        private MemoryMetricsClient MetricsClient { get; }
        public ServerModule(OpenApi api, MemoryMetricsClient metricsClient) : base("/server")
        {
            Api = api;
            MetricsClient = metricsClient;
            
            Get("/", Root);
            Get("/levels", GetLevels);
            Get("/plugins", GetPlugins);
        }

        private object GetPlugins(dynamic arg)
        {
            return Response.AsJson(Api.PluginManager.GetLoadedPlugins().Select(x => new PluginInfo(x)).ToArray());
        }

        private object GetLevels(dynamic arg)
        {
            List<LevelInfo> levelInfos = new List<LevelInfo>();

            var levels = Api.LevelManager.Levels.ToArray();
            foreach (var level in levels)
            {
                var players = new ExpandoObject();
                var playerDict = players as IDictionary<string, object>;
                
                LevelInfo info = new LevelInfo();
                info.Id = level.LevelId;
                info.LoadedChunks = level.GetLoadedChunks().Length;
                info.PlayerCount = level.PlayerCount;
                
                foreach (var player in level.GetAllPlayers().Cast<OpenPlayer>())
                {
                    playerDict.Add(player.CertificateData.ExtraData.Xuid, new PlayerInfo(player));
                }


                info.Players = players;
                levelInfos.Add(info);
            }
            
            return Response.AsJson(levelInfos.ToArray());
        }

        private object Root(dynamic arg)
        {
            int availableWorkerThreads;
            int availablePortThreads;
            ThreadPool.GetAvailableThreads(out availableWorkerThreads, out availablePortThreads);

            int maxWorkerThreads;
            int maxPortThreads;
            ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxPortThreads);

            return Response.AsJson(new ServerInfo()
            {
                Ram = MetricsClient.GetMetrics(),
                Threads = new ServerInfo.MinMax()
                {
                    Total = maxWorkerThreads,
                    Free = availableWorkerThreads,
                    Used = maxWorkerThreads - availableWorkerThreads
                },
                CompletionPorts = new ServerInfo.MinMax()
                {
                    Free = availablePortThreads,
                    Total = maxPortThreads,
                    Used = maxPortThreads - availablePortThreads
                }
            });
        }
    }
}