using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using log4net;
using MiNET.Utils;
using Newtonsoft.Json;

namespace OpenAPI.Utils.ResourcePacks
{
    public class ResourcePackProvider
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ResourcePackProvider));
        
        private OpenApi Api { get; }
        public bool HasData { get; private set; } = false;
        public bool MustAccept { get; private set; } = false;
        private Dictionary<ResourcePackManifest, byte[]> Manifests { get; } = new Dictionary<ResourcePackManifest, byte[]>();
        public ResourcePackProvider(OpenApi api)
        {
            Api = api;
        }

        public uint GetChunkCount(string packageId, uint maxChunKSize, out ResourcePackManifest manifest, out ulong size, out byte[] hash)
        {
            var m = Manifests.FirstOrDefault(x => x.Key.Header.Uuid.Equals(packageId));
            if (m.Value != default)
            {
                manifest = m.Key;
                size = (ulong) m.Value.Length;
                
                SHA256 sha = SHA256.Create();
                sha.Initialize();

                hash = sha.ComputeHash(m.Value);
                
                sha.Dispose();
                
                return (uint)Math.Ceiling((double)((double)m.Value.Length / (double)maxChunKSize));
            }

            size = 0;
            manifest = null;
            hash = null;
            return 0;
        }

        public byte[] GetResourcePackChunk(string packageId, uint chunkIndex, uint maxChunkSize)
        {
            var m = Manifests.FirstOrDefault(x => x.Key.Header.Uuid.Equals(packageId));
            if (m.Value != default)
            {
                var position = maxChunkSize * chunkIndex;
                using (MemoryStream ms = new MemoryStream(m.Value))
                {
                    ms.Position = position;

                    using (MemoryStream target = new MemoryStream())
                    {
                        int readTotal = 0;
                        int read;
                        do
                        {
                            byte[] buffer = new byte[128];
                            read = ms.Read(buffer, 0, buffer.Length);

                            if (read > 0)
                            {
                                readTotal += read;
                                target.Write(buffer, 0, read);
                            }

                        } while (read > 0);

                        var data = target.ToArray();
                        return data.Take((int) Math.Min(data.Length, maxChunkSize)).ToArray();
                    }
                }
               // byte[] target = new byte[];
            }

            return null;
        }

        public void UseResourcePack(string file, bool required)
        {
            if (File.Exists(file))
            {
                ResourcePackManifest packManifest = null;
                using (var fs = File.OpenRead(file))
                {
                    using (ZipArchive zipArchive = new ZipArchive(fs, ZipArchiveMode.Read, true))
                    {
                        try
                        {
                            var manifest = zipArchive.GetEntry("manifest.json");
                            if (manifest != null)
                            {
                                string manifestData;
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    using (var manifestStream = manifest.Open())
                                    {
                                        int read;
                                        do
                                        {
                                            byte[] buffer = new byte[128];
                                            read = manifestStream.Read(buffer, 0, buffer.Length);
                                            
                                            if (read > 0)
                                                ms.Write(buffer, 0, read);
                                            
                                        } while (read > 0);
                                    }

                                    manifestData = Encoding.UTF8.GetString(ms.ToArray());
                                }

                                packManifest =
                                    JsonConvert.DeserializeObject<ResourcePackManifest>(manifestData);
                            }
                        }
                        catch (Exception ex)
                        {
                            //Could not load resourcepack
                            Log.Warn($"Failed to load resource pack: {ex.ToString()}");
                        }
                    }

                    if (packManifest != null)
                    {
                        fs.Position = 0;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            int read;
                            do
                            {
                                byte[] buffer = new byte[128];
                                read = fs.Read(buffer, 0, buffer.Length);

                                if (read > 0)
                                    ms.Write(buffer, 0, read);

                            } while (read > 0);

                            Manifests.Add(packManifest, ms.ToArray());
                            Log.Info($"Resourcepack added: {packManifest.Header.Name} - {packManifest.Header.Version}");
                        }
                    }
                    else
                    {
                        Log.Warn($"PackManifest was null.");
                    }
                }

                MustAccept = required;

                HasData = Manifests.Count > 0;
            }
            else
            {
                Log.Warn($"Resourcepack file does not exist: {file}");
            }
        }

        public IEnumerable<ResourcePackInfo> GetResourcePackInfos()
        {
            foreach (var manifest in Manifests)
            {
                yield return new ResourcePackInfo()
                {
                    Size = (ulong) manifest.Value.Length,
                    HasScripts = false,
                    PackIdVersion = new PackIdVersion()
                    {
                        Id = manifest.Key.Header.Uuid.ToString(),
                        Version = string.Join('.', manifest.Key.Header.Version)
                    }
                };
            }
        }
    }
}