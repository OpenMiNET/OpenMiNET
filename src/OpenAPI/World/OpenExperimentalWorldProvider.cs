using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Schema;
using fNbt;
using log4net;
using MiNET.BlockEntities;
using MiNET.Blocks;
using MiNET.Items;
using MiNET.Net;
using MiNET.Utils;
using MiNET.Worlds;

namespace OpenAPI.World
{
    public class OpenExperimentalWorldProvider : IWorldProvider
    {
        private bool _isInitialized = false;
        private object _initializeSync = new object();

        public void Initialize()
        {
            if (_isInitialized) return; // Quick exit

            lock (_initializeSync)
            {
                if (_isInitialized) return;

                BasePath = BasePath ?? Config.GetProperty("PCWorldFolder", "World").Trim();

                NbtFile file = new NbtFile();
                var levelFileName = Path.Combine(BasePath, "level.dat");
                if (File.Exists(levelFileName))
                {
                    file.LoadFromFile(levelFileName);
                    NbtTag dataTag = file.RootTag["Data"];
                    LevelInfo = new LevelInfo(dataTag);
                }
                else
                {
                    Log.Warn($"No level.dat found at {levelFileName}. Creating empty.");
                    LevelInfo = new LevelInfo();
                }

                switch (Dimension)
                {
                    case Dimension.Overworld:
                        break;
                    case Dimension.Nether:
                        BasePath = Path.Combine(BasePath, @"DIM-1");
                        break;
                    case Dimension.TheEnd:
                        BasePath = Path.Combine(BasePath, @"DIM1");
                        break;
                }

                // MissingChunkProvider?.Initialize();

                _isInitialized = true;
            }
        }

        public bool IsCaching { get; private set; } = true;

        public long GetTime()
        {
            return LevelInfo.Time;
        }

        public long GetDayTime()
        {
            return LevelInfo.DayTime;
        }

        public string GetName()
        {
            return LevelInfo.LevelName;
        }

        public Vector3 GetSpawnPoint()
        {
            var spawnPoint = new Vector3(LevelInfo.SpawnX, LevelInfo.SpawnY + 2 /* + WaterOffsetY*/, LevelInfo.SpawnZ);
            if (Dimension == Dimension.TheEnd)
            {
                spawnPoint = new Vector3(100, 49, 0);
            }
            else if (Dimension == Dimension.Nether)
            {
                spawnPoint = new Vector3(0, 80, 0);
            }

            if (spawnPoint.Y > 256) spawnPoint.Y = 255;

            return spawnPoint;
        }

        private static readonly Random getrandom = new Random();
        private static readonly object syncLock = new object();

        private static readonly OpenSimplexNoise OpenNoise = new OpenSimplexNoise("a-seed".GetHashCode());

        private static readonly ILog Log = LogManager.GetLogger(typeof(OpenExperimentalWorldProvider));

        private static readonly Regex _regex =
            new Regex(@"^((\{""extra"":\[)?)(\{""text"":"".*?""})(],)?(""text"":"".*?""})?$");

        public static readonly Dictionary<int, Tuple<int, Func<int, byte, byte>>> Convert;
        private readonly int Seed;

        public ConcurrentDictionary<ChunkCoordinates, ChunkColumn> _chunkCache =
            new ConcurrentDictionary<ChunkCoordinates, ChunkColumn>();

        private HighPrecisionTimer _tickerHighPrecisionTimer;

        public void Run(object o)
        {
            if (Level != null && _isInitialized)
            {
                // Log.Info("RE-ADDER RAN=========================================");
                List<String> Completed = new List<string>();
                foreach (var v in new Dictionary<String, List<Block>>(BlocksToBeAddedDuringChunkGeneration))
                {
                    var chunkkey = v.Key;
                    ChunkCoordinates cc = new ChunkCoordinates(int.Parse(chunkkey.Split("|")[0]),
                        int.Parse(chunkkey.Split("|")[1]));
                    var c = GenerateChunkColumn(cc, true);
                    if (c != null)
                    {
                        foreach (var block in BlocksToBeAddedDuringChunkGeneration[v.Key])
                        {
                            var ccc = block.Coordinates;
                            int ax = (ccc.X % 16);
                            if (ax < 0) ax += 16;
                            int az = (ccc.Z % 16);
                            if (az < 0) az += 16;
                            if(c.GetBlockId(ax,ccc.Y,az) != new Wood().Id)
                                c.SetBlock(ax, ccc.Y, az, block);
                            // Log.Info($"=================================SETTING BLOCK AT {c} || {ax} {az} || {block.Id}");
                        }
                        // Completed.Add(v.Key);
                        BlocksToBeAddedDuringChunkGeneration[chunkkey].Clear();
                    }
                }

                // foreach (var comp in Completed)
                // {
                //     BlocksToBeAddedDuringChunkGeneration[comp].Clear();
                // }
            }
        }

        private float dirtBaseHeight = 3;
        private float dirtNoise = 0.004f;
        private float dirtNoiseHeight = 10;

        private float stoneBaseHeight = 50;
        private float stoneBaseNoise = 0.05f;
        private float stoneBaseNoiseHeight = 20;
        private float stoneMinHeight = 20;
        private float stoneMountainFrequency = 0.008f;

        private float stoneMountainHeight = 48;
        private int waterLevel = 25;

        public OpenExperimentalWorldProvider(int seed)
        {
            Seed = seed;
            BasePath = Config.GetProperty("PCWorldFolder", "World").Trim();

            _tickerHighPrecisionTimer = new HighPrecisionTimer(50 * 10, Run, false, false);
        }

        public string BasePath { get; set; }
        public OpenLevel Level { get; set; }

        public LevelInfo LevelInfo { get; private set; }

        public Dimension Dimension { get; set; }


        public Queue<Block> LightSources { get; set; } = new Queue<Block>();
        public bool ReadSkyLight { get; set; } = true;

        public bool ReadBlockLight { get; set; } = true;


        public float[] getChunkRTH(ChunkColumn chunk)
        {
            //CALCULATE RAIN
            var rainnoise = new FastNoise(123123);
            rainnoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
            rainnoise.SetFrequency(.007f); //.015
            rainnoise.SetFractalType(FastNoise.FractalType.FBM);
            rainnoise.SetFractalOctaves(1);
            rainnoise.SetFractalLacunarity(.25f);
            rainnoise.SetFractalGain(1);
            //CALCULATE TEMP
            var tempnoise = new FastNoise(123123 + 1);
            tempnoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
            tempnoise.SetFrequency(.004f); //.015f
            tempnoise.SetFractalType(FastNoise.FractalType.FBM);
            tempnoise.SetFractalOctaves(1);
            tempnoise.SetFractalLacunarity(.25f);
            tempnoise.SetFractalGain(1);
            //CALCULATE HEIGHT
            // var heightnoise = new FastNoise(123123 + 2);
            // heightnoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
            // heightnoise.SetFrequency(.015f);
            // heightnoise.SetFractalType(FastNoise.FractalType.FBM);
            // heightnoise.SetFractalOctaves(1);
            // heightnoise.SetFractalLacunarity(.25f);
            // heightnoise.SetFractalGain(1);


            var rain = rainnoise.GetNoise(chunk.X, chunk.Z) + 1;
            var temp = tempnoise.GetNoise(chunk.X, chunk.Z) + 1;
            var height = GetNoise(chunk.X, chunk.Z, 0.015f, 2);
            ;
            return new[] {rain, temp, height};
        }


        public ChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates, bool cacheOnly = false)
        {
            return GenerateChunkColumn2(chunkCoordinates, true, cacheOnly);
        }

        public ChunkColumn GenerateChunkColumn2(ChunkCoordinates chunkCoordinates, bool smooth = true,
            bool cacheOnly = false)
        {
            var v = NOWGenerateChunkColumn(chunkCoordinates, smooth, cacheOnly);
            if (v == null) return null;
            // Console.WriteLine($"CHUNK IS NOT NULL  {chunkCoordinates}");

            //Move into method About 2 \/\/\/
            v.Generated = true;
            if (v.Generated && !v.SurfaceItemsGenerated) v = PreGenerateSurfaceItems(this, v, null);

            // Console.WriteLine($"Done PreGen! {chunkCoordinates}");
            _chunkCache[chunkCoordinates] = v;
            return v;
        }

        public static void AddBlocksToBeAddedDuringChunkGeneration(ChunkCoordinates chunkCoordinates,
            List<Block> blocks)
        {
            var c = chunkCoordinates.X + "|" + chunkCoordinates.Z;
            if (!BlocksToBeAddedDuringChunkGeneration.ContainsKey(c))
                BlocksToBeAddedDuringChunkGeneration[c] = new List<Block>();

            foreach (var block in blocks)
            {
                OpenExperimentalWorldProvider
                    .BlocksToBeAddedDuringChunkGeneration[c].Add(block);
            }
        }

        public static void AddBlocksToBeAddedDuringChunkGeneration(String c, List<Block> blocks)
        {
            if (!BlocksToBeAddedDuringChunkGeneration.ContainsKey(c))
                BlocksToBeAddedDuringChunkGeneration[c] = new List<Block>();

            foreach (var block in blocks)
            {
                OpenExperimentalWorldProvider
                    .BlocksToBeAddedDuringChunkGeneration[c].Add(block);
            }
        }

        /// <summary>
        /// Key is X|Z
        /// </summary>
        /// <param name="c"></param>
        /// <param name="block"></param>
        public static void AddBlockToBeAddedDuringChunkGeneration(String c, Block block)
        {
            if (!BlocksToBeAddedDuringChunkGeneration.ContainsKey(c))
                BlocksToBeAddedDuringChunkGeneration[c] = new List<Block>();

            OpenExperimentalWorldProvider
                .BlocksToBeAddedDuringChunkGeneration[c].Add(block);
        }

        public static void AddBlockToBeAddedDuringChunkGeneration(ChunkCoordinates chunkCoordinates, Block block)
        {
            var c = chunkCoordinates.X + "|" + chunkCoordinates.Z;
            if (!BlocksToBeAddedDuringChunkGeneration.ContainsKey(c))
                BlocksToBeAddedDuringChunkGeneration[c] = new List<Block>();

            OpenExperimentalWorldProvider
                .BlocksToBeAddedDuringChunkGeneration[c].Add(block);
        }

        /// <summary>
        /// 
        /// </summary>
        public static Dictionary<String, List<Block>> BlocksToBeAddedDuringChunkGeneration =
            new Dictionary<String, List<Block>>();

        public ChunkColumn NOWGenerateChunkColumn(ChunkCoordinates chunkCoordinates, bool smooth = true,
            bool cacheOnly = false)
        {
            ChunkColumn cachedChunk;
            if (_chunkCache.TryGetValue(chunkCoordinates, out cachedChunk)) return cachedChunk;

            ChunkColumn chunk;
            if (cacheOnly) return null;
            chunk = PreGetChunk(chunkCoordinates, BasePath).Result;
            if (chunk != null)
            {
                _chunkCache[chunkCoordinates] = chunk;
                return chunk;
            }


            chunk = new ChunkColumn();
            chunk.X = chunkCoordinates.X;
            chunk.Z = chunkCoordinates.Z;
            var rth = getChunkRTH(chunk);

            // Console.WriteLine("STARTING POPULATIOaN");
            chunk = PopulateChunk(this, chunk, rth).Result;

            if (smooth)
            {
                // Console.WriteLine("STARTING SMOOTHING");
                chunk = SmoothChunk(this, chunk, rth).Result;
            }
            
            return chunk;
        }


        public static void CleanSignText(NbtCompound blockEntityTag, string tagName)
        {
            var text = blockEntityTag[tagName].StringValue;
            var replace = /*Regex.Unescape*/_regex.Replace(text, "$3");
            blockEntityTag[tagName] = new NbtString(tagName, replace);
        }

        public void SaveLevelInfo(LevelInfo level)
        {
            if (Dimension != Dimension.Overworld) return;

            var leveldat = Path.Combine(BasePath, "level.dat");

            if (!Directory.Exists(BasePath))
                Directory.CreateDirectory(BasePath);
            else if (File.Exists(leveldat))
                return; // What if this is changed? Need a dirty flag on this

            if (LevelInfo.SpawnY <= 0) LevelInfo.SpawnY = 256;

            var file = new NbtFile();
            NbtTag dataTag = new NbtCompound("Data");
            var rootTag = (NbtCompound) file.RootTag;
            rootTag.Add(dataTag);
            level.SaveToNbt(dataTag);
            file.SaveToFile(leveldat, NbtCompression.GZip);
        }

        public int SaveChunks()
        {
            return 0;
            var count = 0;
            try
            {
                lock (_chunkCache)
                {
                    SaveLevelInfo(new LevelInfo());

                    var regions = new Dictionary<Tuple<int, int>, List<ChunkColumn>>();
                    foreach (var chunkColumn in _chunkCache.OrderBy(pair => pair.Key.X >> 5)
                        .ThenBy(pair => pair.Key.Z >> 5))
                    {
                        var regionKey = new Tuple<int, int>(chunkColumn.Key.X >> 5, chunkColumn.Key.Z >> 5);
                        if (!regions.ContainsKey(regionKey)) regions.Add(regionKey, new List<ChunkColumn>());

                        regions[regionKey].Add(chunkColumn.Value);
                    }

                    var tasks = new List<Task>();
                    foreach (var region in regions.OrderBy(pair => pair.Key.Item1).ThenBy(pair => pair.Key.Item2))
                    {
                        var task = new Task(delegate
                        {
                            var chunks = region.Value;
                            foreach (var chunkColumn in chunks)
                                if (chunkColumn != null && chunkColumn.NeedSave)
                                {
                                    SaveChunk(chunkColumn, BasePath);
                                    count++;
                                }
                        });
                        task.Start();
                        tasks.Add(task);
                    }

                    Task.WaitAll(tasks.ToArray());

                    //foreach (var chunkColumn in _chunkCache.OrderBy(pair => pair.Key.X >> 5).ThenBy(pair => pair.Key.Z >> 5))
                    //{
                    //	if (chunkColumn.Value != null && chunkColumn.Value.NeedSave)
                    //	{
                    //		SaveChunk(chunkColumn.Value, BasePath);
                    //		count++;
                    //	}
                    //}
                }
            }
            catch (Exception e)
            {
                Log.Error("saving chunks", e);
            }

            return count;
        }

        public static void SaveChunk(ChunkColumn chunk, string basePath)
        {
            // WARNING: This method does not consider growing size of the chunks. Needs refactoring to find
            // free sectors and clear up old ones. It works fine as long as no dynamic data is written
            // like block entity data (signs etc).

            var time = Stopwatch.StartNew();

            chunk.NeedSave = false;

            var coordinates = new ChunkCoordinates(chunk.X, chunk.Z);

            var width = 32;
            var depth = 32;

            var rx = coordinates.X >> 5;
            var rz = coordinates.Z >> 5;

            var filePath = Path.Combine(basePath,
                string.Format(@"region{2}r.{0}.{1}.mca", rx, rz, Path.DirectorySeparatorChar));

            Log.Debug($"Save chunk X={chunk.X}, Z={chunk.Z} to {filePath}");

            if (!File.Exists(filePath))
            {
                // Make sure directory exist
                Directory.CreateDirectory(Path.Combine(basePath, "region"));

                // Create empty region file
                using (var regionFile = File.Open(filePath, FileMode.CreateNew))
                {
                    var buffer = new byte[8192];
                    regionFile.Write(buffer, 0, buffer.Length);
                }
            }

            var testTime = new Stopwatch();

            using (var regionFile = File.Open(filePath, FileMode.Open))
            {
                // Region files begin with an 8kiB header containing information about which chunks are present in the region file, 
                // when they were last updated, and where they can be found.
                var buffer = new byte[8192];
                regionFile.Read(buffer, 0, buffer.Length);

                var xi = coordinates.X % width;
                if (xi < 0) xi += 32;
                var zi = coordinates.Z % depth;
                if (zi < 0) zi += 32;
                var tableOffset = (xi + zi * width) * 4;

                regionFile.Seek(tableOffset, SeekOrigin.Begin);

                // Location information for a chunk consists of four bytes split into two fields: the first three bytes are a(big - endian) offset in 4KiB sectors 
                // from the start of the file, and a remaining byte which gives the length of the chunk(also in 4KiB sectors, rounded up).
                var offsetBuffer = new byte[4];
                regionFile.Read(offsetBuffer, 0, 3);
                Array.Reverse(offsetBuffer);
                var offset = BitConverter.ToInt32(offsetBuffer, 0) << 4;
                var sectorCount = (byte) regionFile.ReadByte();

                testTime.Restart(); // RESTART

                // Seriaize NBT to get lenght
                var nbt = CreateNbtFromChunkColumn(chunk);

                testTime.Stop();

                var nbtBuf = nbt.SaveToBuffer(NbtCompression.ZLib);
                var nbtLength = nbtBuf.Length;
                var nbtSectorCount = (byte) Math.Ceiling(nbtLength / 4096d);

                // Don't write yet, just use the lenght

                if (offset == 0 || sectorCount == 0 || nbtSectorCount > sectorCount)
                {
                    if (Log.IsDebugEnabled)
                        if (sectorCount != 0)
                            Log.Warn(
                                $"Creating new sectors for this chunk even tho it existed. Old sector count={sectorCount}, new sector count={nbtSectorCount} (lenght={nbtLength})");

                    regionFile.Seek(0, SeekOrigin.End);
                    offset = (int) ((int) regionFile.Position & 0xfffffff0);

                    regionFile.Seek(tableOffset, SeekOrigin.Begin);

                    var bytes = BitConverter.GetBytes(offset >> 4);
                    Array.Reverse(bytes);
                    regionFile.Write(bytes, 0, 3);
                    regionFile.WriteByte(nbtSectorCount);
                }

                var lenghtBytes = BitConverter.GetBytes(nbtLength + 1);
                Array.Reverse(lenghtBytes);

                regionFile.Seek(offset, SeekOrigin.Begin);
                regionFile.Write(lenghtBytes, 0, 4); // Lenght
                regionFile.WriteByte(0x02); // Compression mode zlib

                regionFile.Write(nbtBuf, 0, nbtBuf.Length);

                int reminder;
                Math.DivRem(nbtLength + 4, 4096, out reminder);

                var padding = new byte[4096 - reminder];
                if (padding.Length > 0) regionFile.Write(padding, 0, padding.Length);

                testTime.Stop(); // STOP

                Log.Warn(
                    $"Took {time.ElapsedMilliseconds}ms to save. And {testTime.ElapsedMilliseconds}ms to generate bytes from NBT");
            }
        }


        public static NbtFile CreateNbtFromChunkColumn(ChunkColumn chunk)
        {
            var nbt = new NbtFile();

            var levelTag = new NbtCompound("Level");
            var rootTag = (NbtCompound) nbt.RootTag;
            rootTag.Add(levelTag);

            levelTag.Add(new NbtByte("MCPE BID", 1)); // Indicate that the chunks contain PE block ID's.

            levelTag.Add(new NbtInt("xPos", chunk.X));
            levelTag.Add(new NbtInt("zPos", chunk.Z));
            levelTag.Add(new NbtByteArray("Biomes", chunk.biomeId));

            var sectionsTag = new NbtList("Sections", NbtTagType.Compound);
            levelTag.Add(sectionsTag);

            for (var i = 0; i < 16; i++)
            {
                var subChunk = chunk[i];
                if (subChunk.IsAllAir())
                {
                    if (i == 0) Log.Debug($"All air bottom chunk? {subChunk.GetBlockId(0, 0, 0)}");
                    continue;
                }

                var sectionTag = new NbtCompound();
                sectionsTag.Add(sectionTag);
                sectionTag.Add(new NbtByte("Y", (byte) i));

                var blocks = new byte[4096];
                var data = new byte[2048];
                var blockLight = new byte[2048];
                var skyLight = new byte[2048];

                {
                    for (var x = 0; x < 16; x++)
                    for (var z = 0; z < 16; z++)
                    for (var y = 0; y < 16; y++)
                    {
                        var anvilIndex = y * 16 * 16 + z * 16 + x;
                        var blockId = (byte) subChunk.GetBlockId(x, y, z);
                        if (blockId < 0) blockId = 0;
                        blocks[anvilIndex] = blockId;
                        //SetNibble4(data, anvilIndex, section.GetMetadata(x, y, z));
                        SetNibble4(blockLight, anvilIndex, subChunk.GetBlocklight(x, y, z));
                        SetNibble4(skyLight, anvilIndex, subChunk.GetSkylight(x, y, z));
                    }
                }
                sectionTag.Add(new NbtByteArray("BlocSubChunk.cs:154ks", blocks));
                sectionTag.Add(new NbtByteArray("Data", data));
                sectionTag.Add(new NbtByteArray("BlockLight", blockLight));
                sectionTag.Add(new NbtByteArray("SkyLight", skyLight));
            }

            var heights = new int[256];
            for (var h = 0; h < heights.Length; h++) heights[h] = chunk.height[h];
            levelTag.Add(new NbtIntArray("HeightMap", heights));

            // TODO: Save entities
            var entitiesTag = new NbtList("Entities", NbtTagType.Compound);
            levelTag.Add(entitiesTag);

            var blockEntitiesTag = new NbtList("TileEntities", NbtTagType.Compound);
            foreach (var blockEntityNbt in chunk.BlockEntities.Values)
            {
                var nbtClone = (NbtCompound) blockEntityNbt.Clone();
                nbtClone.Name = null;
                blockEntitiesTag.Add(nbtClone);
            }

            levelTag.Add(blockEntitiesTag);

            levelTag.Add(new NbtList("TileTicks", NbtTagType.Compound));

            return nbt;
        }


        /// <summary>
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="basePath"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<ChunkColumn> PreGetChunk(ChunkCoordinates coordinates, string basePath)
        {
            return await GetChunk(coordinates, basePath);
        }

        public async Task<ChunkColumn> GetChunk(ChunkCoordinates coordinates, string basePath)
        {
            try
            {
                var width = 32;
                var depth = 32;

                var rx = coordinates.X >> 5;
                var rz = coordinates.Z >> 5;

                var filePath = Path.Combine(basePath,
                    string.Format(@"region{2}r.{0}.{1}.mca", rx, rz, Path.DirectorySeparatorChar));

                if (!File.Exists(filePath)) return null;

                using (var regionFile = File.OpenRead(filePath))
                {
                    var buffer = new byte[8192];

                    regionFile.Read(buffer, 0, 8192);

                    var xi = coordinates.X % width;
                    if (xi < 0) xi += 32;
                    var zi = coordinates.Z % depth;
                    if (zi < 0) zi += 32;
                    var tableOffset = (xi + zi * width) * 4;

                    regionFile.Seek(tableOffset, SeekOrigin.Begin);

                    var offsetBuffer = new byte[4];
                    regionFile.Read(offsetBuffer, 0, 3);
                    Array.Reverse(offsetBuffer);
                    var offset = BitConverter.ToInt32(offsetBuffer, 0) << 4;

                    var bytes = BitConverter.GetBytes(offset >> 4);
                    Array.Reverse(bytes);
                    if (offset != 0 && offsetBuffer[0] != bytes[0] && offsetBuffer[1] != bytes[1] &&
                        offsetBuffer[2] != bytes[2])
                        throw new Exception(
                            $"Not the same buffer\n{Packet.HexDump(offsetBuffer)}\n{Packet.HexDump(bytes)}");

                    var length = regionFile.ReadByte();

                    if (offset == 0 || length == 0) return null;

                    regionFile.Seek(offset, SeekOrigin.Begin);
                    var waste = new byte[4];
                    regionFile.Read(waste, 0, 4);
                    var compressionMode = regionFile.ReadByte();

                    if (compressionMode != 0x02)
                        throw new Exception(
                            $"CX={coordinates.X}, CZ={coordinates.Z}, NBT wrong compression. Expected 0x02, got 0x{compressionMode:X2}. " +
                            $"Offset={offset}, length={length}\n{Packet.HexDump(waste)}");

                    var nbt = new NbtFile();
                    nbt.LoadFromStream(regionFile, NbtCompression.ZLib);

                    var dataTag = (NbtCompound) nbt.RootTag["Level"];

                    var isPocketEdition = false;
                    if (dataTag.Contains("MCPE BID")) isPocketEdition = dataTag["MCPE BID"].ByteValue == 1;

                    var sections = dataTag["Sections"] as NbtList;

                    var chunk = new ChunkColumn
                    {
                        X = coordinates.X,
                        Z = coordinates.Z,
                        biomeId = dataTag["Biomes"].ByteArrayValue,
                        IsAllAir = true
                    };

                    if (chunk.biomeId.Length > 256) throw new Exception();

                    NbtTag heights = dataTag["HeightMap"] as NbtIntArray;
                    if (heights != null)
                    {
                        var intHeights = heights.IntArrayValue;
                        for (var i = 0; i < 256; i++) chunk.height[i] = (short) intHeights[i];
                    }

                    // This will turn into a full chunk column
                    foreach (var sectionTag in sections) ReadSection(sectionTag, chunk, !isPocketEdition);

                    var entities = dataTag["Entities"] as NbtList;

                    var blockEntities = dataTag["TileEntities"] as NbtList;
                    if (blockEntities != null)
                        foreach (var nbtTag in blockEntities)
                        {
                            var blockEntityTag = (NbtCompound) nbtTag.Clone();
                            var entityId = blockEntityTag["id"].StringValue;
                            var x = blockEntityTag["x"].IntValue;
                            var y = blockEntityTag["y"].IntValue;
                            var z = blockEntityTag["z"].IntValue;

                            if (entityId.StartsWith("minecraft:"))
                            {
                                var id = entityId.Split(':')[1];

                                entityId = id.First().ToString().ToUpper() + id.Substring(1);
                                if (entityId == "Flower_pot") entityId = "FlowerPot";
                                else if (entityId == "Shulker_box") entityId = "ShulkerBox";

                                blockEntityTag["id"] = new NbtString("id", entityId);
                            }

                            var blockEntity = BlockEntityFactory.GetBlockEntityById(entityId);

                            if (blockEntity != null)
                            {
                                blockEntityTag.Name = string.Empty;
                                blockEntity.Coordinates = new BlockCoordinates(x, y, z);

                                if (blockEntity is Sign)
                                {
                                    if (Log.IsDebugEnabled) Log.Debug($"Loaded sign block entity\n{blockEntityTag}");
                                    // Remove the JSON stuff and get the text out of extra data.
                                    // TAG_String("Text2"): "{"extra":["10c a loaf!"],"text":""}"
                                    CleanSignText(blockEntityTag, "Text1");
                                    CleanSignText(blockEntityTag, "Text2");
                                    CleanSignText(blockEntityTag, "Text3");
                                    CleanSignText(blockEntityTag, "Text4");
                                }
                                else if (blockEntity is ChestBlockEntity || blockEntity is ShulkerBoxBlockEntity)
                                {
                                    if (blockEntity is ShulkerBoxBlockEntity)
                                    {
                                        //var meta = chunk.GetMetadata(x & 0x0f, y, z & 0x0f);

                                        //blockEntityTag["facing"] = new NbtByte("facing", (byte) (meta >> 4));

                                        //chunk.SetBlock(x & 0x0f, y, z & 0x0f, 218,(byte) (meta - ((byte) (meta >> 4) << 4)));
                                    }

                                    var items = (NbtList) blockEntityTag["Items"];

                                    if (items != null)
                                        for (byte i = 0; i < items.Count; i++)
                                        {
                                            var item = (NbtCompound) items[i];

                                            var itemName = item["id"].StringValue;
                                            if (itemName.StartsWith("minecraft:"))
                                            {
                                                var id = itemName.Split(':')[1];

                                                itemName = id.First().ToString().ToUpper() + id.Substring(1);
                                            }

                                            var itemId = ItemFactory.GetItemIdByName(itemName);
                                            item.Remove("id");
                                            item.Add(new NbtShort("id", itemId));
                                        }
                                }
                                else if (blockEntity is BedBlockEntity)
                                {
                                    var color = blockEntityTag["color"];
                                    blockEntityTag.Remove("color");
                                    blockEntityTag.Add(color is NbtByte
                                        ? color
                                        : new NbtByte("color", (byte) color.IntValue));
                                }
                                else if (blockEntity is FlowerPotBlockEntity)
                                {
                                    var itemName = blockEntityTag["Item"].StringValue;
                                    if (itemName.StartsWith("minecraft:"))
                                    {
                                        var id = itemName.Split(':')[1];

                                        itemName = id.First().ToString().ToUpper() + id.Substring(1);
                                    }

                                    var itemId = ItemFactory.GetItemIdByName(itemName);
                                    blockEntityTag.Remove("Item");
                                    blockEntityTag.Add(new NbtShort("item", itemId));

                                    var data = blockEntityTag["Data"].IntValue;
                                    blockEntityTag.Remove("Data");
                                    blockEntityTag.Add(new NbtInt("mData", data));
                                }
                                else
                                {
                                    if (Log.IsDebugEnabled) Log.Debug($"Loaded block entity\n{blockEntityTag}");
                                    blockEntity.SetCompound(blockEntityTag);
                                    blockEntityTag = blockEntity.GetCompound();
                                }

                                chunk.SetBlockEntity(new BlockCoordinates(x, y, z), blockEntityTag);
                            }
                            else
                            {
                                if (Log.IsDebugEnabled) Log.Debug($"Loaded unknown block entity\n{blockEntityTag}");
                            }
                        }

                    //NbtList tileTicks = dataTag["TileTicks"] as NbtList;

                    // if (Dimension == Dimension.Overworld && Config.GetProperty("CalculateLights", false))
                    // {
                    //     chunk.RecalcHeight();
                    //
                    //     var blockAccess = new SkyLightBlockAccess(this, chunk);
                    //     new SkyLightCalculations().RecalcSkyLight(chunk, blockAccess);
                    //     //TODO: Block lights.
                    // }

                    chunk.IsDirty = false;
                    chunk.NeedSave = false;

                    return chunk;
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error While Loading chunk {coordinates}", e);
                return null;
            }
        }

        public bool HaveNether()
        {
            return false;
        }

        public bool HaveTheEnd()
        {
            return false;
        }

        private static void SetNibble4(byte[] arr, int index, byte value)
        {
            value &= 0xF;
            var idx = index >> 1;
            arr[idx] &= (byte) (0xF << (((index + 1) & 1) * 4));
            arr[idx] |= (byte) (value << ((index & 1) * 4));
        }

        private static byte Nibble4(byte[] arr, int index)
        {
            return (byte) ((arr[index >> 1] >> ((index & 1) * 4)) & 0xF);
        }

        public void ReadSection(NbtTag sectionTag, ChunkColumn chunkColumn, bool convertBid = true)
        {
            int sectionIndex = sectionTag["Y"].ByteValue;
            var blocks = sectionTag["Blocks"].ByteArrayValue;
            var data = sectionTag["Data"].ByteArrayValue;
            var addTag = sectionTag["Add"];
            var adddata = new byte[2048];
            if (addTag != null) adddata = addTag.ByteArrayValue;
            var blockLight = sectionTag["BlockLight"].ByteArrayValue;
            var skyLight = sectionTag["SkyLight"].ByteArrayValue;

            var subChunk = chunkColumn[sectionIndex];

            for (var x = 0; x < 16; x++)
            for (var z = 0; z < 16; z++)
            for (var y = 0; y < 16; y++)
            {
                var yi = (sectionIndex << 4) + y;

                var anvilIndex = (y << 8) + (z << 4) + x;
                var blockId = blocks[anvilIndex] + (Nibble4(adddata, anvilIndex) << 8);

                // Anvil to PE friendly converstion

                Func<int, byte, byte> dataConverter = (i, b) => b; // Default no-op converter
                if (convertBid && Convert.ContainsKey(blockId))
                {
                    dataConverter = Convert[blockId].Item2;
                    blockId = Convert[blockId].Item1;
                }
                //else
                //{
                //	if (BlockFactory.GetBlockById((byte)blockId).GetType() == typeof(Block))
                //	{
                //		Log.Warn($"No block implemented for block ID={blockId}, Meta={data}");
                //		//blockId = 57;
                //	}
                //}

                chunkColumn.IsAllAir &= blockId == 0;
                if (blockId > 255)
                {
                    Log.Warn($"Failed mapping for block ID={blockId}, Meta={data}");
                    blockId = 41;
                }

                if (yi == 0 && (blockId == 8 || blockId == 9)) blockId = 7; // Bedrock under water

                var metadata = Nibble4(data, anvilIndex);
                metadata = dataConverter(blockId, metadata);

                var runtimeId = (int) BlockFactory.GetRuntimeId(blockId, metadata);
                subChunk.SetBlockByRuntimeId(x, y, z, runtimeId);
                if (ReadBlockLight) subChunk.SetBlocklight(x, y, z, Nibble4(blockLight, anvilIndex));

                if (ReadSkyLight)
                    subChunk.SetSkylight(x, y, z, Nibble4(skyLight, anvilIndex));
                else
                    subChunk.SetSkylight(x, y, z, 0);

                if (blockId == 0) continue;

                if (convertBid && blockId == 3 && metadata == 2)
                {
                    // Dirt Podzol => (Podzol)
                    subChunk.SetBlock(x, y, z, new Podzol());
                    blockId = 243;
                }

                if (BlockFactory.LuminousBlocks[blockId] != 0)
                {
                    var block = BlockFactory.GetBlockById(subChunk.GetBlockId(x, y, z));
                    block.Coordinates = new BlockCoordinates(x + (chunkColumn.X << 4), yi, z + (chunkColumn.Z << 4));
                    subChunk.SetBlocklight(x, y, z, (byte) block.LightLevel);
                    lock (LightSources)
                    {
                        LightSources.Enqueue(block);
                    }
                }
            }
        }

        public async Task<ChunkColumn> SmoothChunk(OpenExperimentalWorldProvider openExperimentalWorldProvider,
            ChunkColumn chunk,
            float[] rth)
        {
            var b = BiomeManager.GetBiome(chunk);
            var a = await b.preSmooth(openExperimentalWorldProvider, chunk, rth);
            return a;
        }

        public async Task<ChunkColumn> PopulateChunk(OpenExperimentalWorldProvider openExperimentalWorldProvider,
            ChunkColumn chunk,
            float[] rth)
        {
            var b = BiomeManager.GetBiome(chunk);
            // var b = new MainBiome();
            var a = await b.prePopulate(openExperimentalWorldProvider, chunk, rth);
            return a;
            // b.PopulateChunk(chunk, rain, temp);

// Console.WriteLine($"GENERATORED YO BITCH >> {chunk.X} {chunk.Z}");
        }

        public ChunkColumn PreGenerateSurfaceItems(OpenExperimentalWorldProvider openExperimentalWorldProvider,
            ChunkColumn chunk,
            float[] rth)
        {
            chunk.SurfaceItemsGenerated = true;
            var a = GenerateSurfaceItems(openExperimentalWorldProvider, chunk, rth);
            return a;
            // b.PopulateChunk(chunk, rain, temp);

// Console.WriteLine($"GENERATORED YO BITCH >> {chunk.X} {chunk.Z}");
        }

        public ChunkColumn GenerateSurfaceItems(OpenExperimentalWorldProvider openExperimentalWorldProvider,
            ChunkColumn chunk,
            float[] rth)
        {
            var b = BiomeManager.GetBiome(chunk);
            // var b = new MainBiome();
            var a = b.GenerateSurfaceItems(openExperimentalWorldProvider, chunk, rth);
            return a;
            // b.PopulateChunk(chunk, rain, temp);

// Console.WriteLine($"GENERATORED YO BITCH >> {chunk.X} {chunk.Z}");
        }

        private void GenerateTree(ChunkColumn chunk, int x, int treebase, int z)
        {
            var treeheight = GetRandomNumber(4, 5);

            chunk.SetBlock(x, treebase + treeheight + 2, z, new Leaves()); //Top leave

            chunk.SetBlock(x, treebase + treeheight + 1, z + 1, new Leaves());
            chunk.SetBlock(x, treebase + treeheight + 1, z - 1, new Leaves());
            chunk.SetBlock(x + 1, treebase + treeheight + 1, z, new Leaves());
            chunk.SetBlock(x - 1, treebase + treeheight + 1, z, new Leaves());

            chunk.SetBlock(x, treebase + treeheight, z + 1, new Leaves());
            chunk.SetBlock(x, treebase + treeheight, z - 1, new Leaves());
            chunk.SetBlock(x + 1, treebase + treeheight, z, new Leaves());
            chunk.SetBlock(x - 1, treebase + treeheight, z, new Leaves());

            chunk.SetBlock(x + 1, treebase + treeheight, z + 1, new Leaves());
            chunk.SetBlock(x - 1, treebase + treeheight, z - 1, new Leaves());
            chunk.SetBlock(x + 1, treebase + treeheight, z - 1, new Leaves());
            chunk.SetBlock(x - 1, treebase + treeheight, z + 1, new Leaves());

            for (var i = 0; i <= treeheight; i++) chunk.SetBlock(x, treebase + i, z, new Log());
        }

        private static int GetRandomNumber(int min, int max)
        {
            lock (syncLock)
            {
                // synchronize
                return getrandom.Next(min, max);
            }
        }

        public static float GetNoise(int x, int z, float scale, int max)
        {
            return (float) ((OpenNoise.Evaluate(x * scale, z * scale) + 1f) * (max / 2f));
            // var heightnoise = new FastNoise(123123 + 2);
            // heightnoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
            // heightnoise.SetFrequency(scale);
            // heightnoise.SetFractalType(FastNoise.FractalType.FBM);
            // heightnoise.SetFractalOctaves(1);
            // heightnoise.SetFractalLacunarity(2);
            // heightnoise.SetFractalGain(.5f);
            // return (heightnoise.GetNoise(x, z)+1 )*(max/2f);
            // return (float)(OpenNoise.Evaluate(x * scale, z * scale) + 1f) * (max / 2f);
        }
    }
}