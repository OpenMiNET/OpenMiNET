using System;
using Newtonsoft.Json;

namespace OpenAPI.Utils.ResourcePacks
{
    public class ResourcePackManifest
    {
        [JsonProperty("format_version")]
        public long FormatVersion { get; set; }

        [JsonProperty("header")]
        public Header Header { get; set; }

        [JsonProperty("modules")]
        public Module[] Modules { get; set; }
    }

    public class Header
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("min_engine_version")]
        public long[] MinEngineVersion { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("uuid")]
        public string Uuid { get; set; }

        [JsonProperty("version")]
        public long[] Version { get; set; }
    }

    public class Module
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("uuid")]
        public string Uuid { get; set; }

        [JsonProperty("version")]
        public long[] Version { get; set; }
    }
}