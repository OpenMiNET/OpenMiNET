using OpenAPI.ManagementApi.Utils;

namespace OpenAPI.ManagementApi
{
    public class ServerInfo
    {
        public MinMax Threads { get; set; }
        public MinMax CompletionPorts { get; set; }
        public MemoryMetrics Ram { get; set; }

        public class MinMax
        {
            public long Total { get; set; }
            public long Free { get; set; }
            public long Used { get; set; }
        }
    }
}