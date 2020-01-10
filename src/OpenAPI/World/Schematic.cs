using System;
using System.Collections.Generic;
using System.IO;
using fNbt;
using log4net;
using MiNET;
using MiNET.Utils;
using MiNET.Worlds;

namespace OpenAPI.World
{
	public class Schematic
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(Schematic));

		private static readonly Dictionary<int, Tuple<int, Func<int, byte, byte>>> Convert;

		static Schematic()
		{
			var air = new Mapper(0, (i, b) => 0);

			Convert = new Dictionary<int, Tuple<int, Func<int, byte, byte>>>
			{
				{36, new NoDataMapper(250)}, // minecraft:piston_extension		=> MovingBlock
				{43, new Mapper(43, (i, b) => (byte) (b == 6 ? 7 : b == 7 ? 6 : b))}, // Fence		=> Fence
				{44, new Mapper(44, (i, b) => (byte) (b == 6 ? 7 : b == 7 ? 6 : b == 14 ? 15 : b == 15 ? 14 : b))}, // Fence		=> Fence
				{
					77, new Mapper(77, delegate(int i, byte b) // stone_button
					{
						switch (b & 0x7f)
						{
							case 0:
								return (byte) BlockFace.Down;
							case 1:
								return (byte) BlockFace.South;
							case 2:
								return (byte) BlockFace.North;
							case 3:
								return (byte) BlockFace.West;
							case 4:
								return (byte) BlockFace.East;
							case 5:
								return (byte) BlockFace.Up;
						}

						return 0;
					})
				},
				{84, new NoDataMapper(25)}, // minecraft:jukebox		=> noteblock
				{85, new Mapper(85, (i, b) => 0)}, // Fence		=> Fence
				{95, new NoDataMapper(241)}, // minecraft:stained_glass	=> Stained Glass
				{96, new Mapper(96, (i, b) => (byte) (((b & 0x04) << 1) | ((b & 0x08) >> 1) | (3 - (b & 0x03))))}, // Trapdoor Fix
				{125, new NoDataMapper(157)}, // minecraft:double_wooden_slab	=> minecraft:double_wooden_slab
				{126, new NoDataMapper(158)}, // minecraft:wooden_slab		=> minecraft:wooden_slab
				{
					143, new Mapper(143, delegate(int i, byte b) // wooden_button
					{
						switch (b & 0x7f)
						{
							case 0:
								return (byte) BlockFace.Down; // 0
							case 1:
								return (byte) BlockFace.South; // 5
							case 2:
								return (byte) BlockFace.North; // 4
							case 3:
								return (byte) BlockFace.West; // 3
							case 4:
								return (byte) BlockFace.East; // 2
							case 5:
								return (byte) BlockFace.Up; // 1
						}

						return 0;
					})
				},
				{157, new NoDataMapper(126)}, // minecraft:activator_rail
				{158, new NoDataMapper(125)}, // minecraft:dropper
				{166, new NoDataMapper(95)}, // minecraft:barrier		=> (Invisible Bedrock)
				{167, new Mapper(167, (i, b) => (byte) (((b & 0x04) << 1) | ((b & 0x08) >> 1) | (3 - (b & 0x03))))}, //Fix iron_trapdoor
				{176, air}, // minecraft:standing_banner		=> Air
				{177, air}, // minecraft:wall_banner		=> Air
				{188, new Mapper(85, (i, b) => 1)}, // Spruce Fence		=> Fence
				{189, new Mapper(85, (i, b) => 2)}, // Birch Fence		=> Fence
				{190, new Mapper(85, (i, b) => 3)}, // Jungle Fence		=> Fence
				{191, new Mapper(85, (i, b) => 5)}, // Dark Oak Fence	=> Fence
				{192, new Mapper(85, (i, b) => 4)}, // Acacia Fence		=> Fence
				{198, new NoDataMapper(208)}, // minecraft:end_rod	=> EndRod
				{199, new NoDataMapper(240)}, // minecraft:chorus_plant
				{202, new Mapper(201, (i, b) => 2)}, // minecraft:purpur_pillar => PurpurBlock:2 (idk why)
				{204, new Mapper(181, (i, b) => 1)}, // minecraft:purpur_double_slab
				{205, new Mapper(182, (i, b) => 1)}, // minecraft:purpur_slab
				{207, new NoDataMapper(244)}, // minecraft:beetroot_block
				{208, new NoDataMapper(198)}, // minecraft:grass_path
				{210, new NoDataMapper(188)}, // repeating_command_block
				{211, new NoDataMapper(189)}, // minecraft:chain_command_block
				{212, new NoDataMapper(297)}, // Frosted Ice
				{218, new NoDataMapper(251)}, // minecraft:observer => Observer
				{219, new Mapper(218, (i, b) => 0)}, // => minecraft:white_shulker_box
				{220, new Mapper(218, (i, b) => 1)}, // => minecraft:orange_shulker_box
				{221, new Mapper(218, (i, b) => 2)}, // => minecraft:magenta_shulker_box
				{222, new Mapper(218, (i, b) => 3)}, // => minecraft:light_blue_shulker_box 
				{223, new Mapper(218, (i, b) => 4)}, // => minecraft:yellow_shulker_box 
				{224, new Mapper(218, (i, b) => 5)}, // => minecraft:lime_shulker_box 
				{225, new Mapper(218, (i, b) => 6)}, // => minecraft:pink_shulker_box 
				{226, new Mapper(218, (i, b) => 7)}, // => minecraft:gray_shulker_box 
				{227, new Mapper(218, (i, b) => 8)}, // => minecraft:light_gray_shulker_box 
				{228, new Mapper(218, (i, b) => 9)}, // => minecraft:cyan_shulker_box 
				{229, new Mapper(218, (i, b) => 10)}, // => minecraft:purple_shulker_box 
				{230, new Mapper(218, (i, b) => 11)}, // => minecraft:blue_shulker_box 
				{231, new Mapper(218, (i, b) => 12)}, // => minecraft:brown_shulker_box 
				{232, new Mapper(218, (i, b) => 13)}, // => minecraft:green_shulker_box 
				{233, new Mapper(218, (i, b) => 14)}, // => minecraft:red_shulker_box 
				{234, new Mapper(218, (i, b) => 15)}, // => minecraft:black_shulker_box 

				{235, new NoDataMapper(220)}, // => minecraft:white_glazed_terracotta
				{236, new NoDataMapper(221)}, // => minecraft:orange_glazed_terracotta
				{237, new NoDataMapper(222)}, // => minecraft:magenta_glazed_terracotta
				{238, new NoDataMapper(223)}, // => minecraft:light_blue_glazed_terracotta
				{239, new NoDataMapper(224)}, // => minecraft:yellow_glazed_terracotta
				{240, new NoDataMapper(225)}, // => minecraft:lime_glazed_terracotta
				{241, new NoDataMapper(226)}, // => minecraft:pink_glazed_terracotta
				{242, new NoDataMapper(227)}, // => minecraft:gray_glazed_terracotta
				{243, new NoDataMapper(228)}, // => minecraft:light_gray_glazed_terracotta
				{244, new NoDataMapper(229)}, // => minecraft:cyan_glazed_terracotta
				{245, new NoDataMapper(219)}, // => minecraft:purple_glazed_terracotta
				{246, new NoDataMapper(231)}, // => minecraft:blue_glazed_terracotta
				{247, new NoDataMapper(232)}, // => minecraft:brown_glazed_terracotta
				{248, new NoDataMapper(233)}, // => minecraft:green_glazed_terracotta
				{249, new NoDataMapper(234)}, // => minecraft:red_glazed_terracotta
				{250, new NoDataMapper(235)}, // => minecraft:black_glazed_terracotta

				{251, new NoDataMapper(236)}, // => minecraft:concrete
				{252, new NoDataMapper(237)}, // => minecraft:concrete_powder
			};
		}

		public byte[] Blocks { get; }
		public NibbleArray Metadata { get; }

		public int Height { get; }
		public int Width { get; }
		public int Depth { get; }

		public Schematic(int height, int width, int depth, byte[] blocks, NibbleArray metadata)
		{
			Height = height;
			Width = width;
			Depth = depth;
			Blocks = blocks;
			Metadata = metadata;
		}

		public Schematic(string fileName) : this(File.OpenRead(fileName)) { }

		public Schematic(Stream stream)
		{
			var schematicFile = new NbtFile();
			schematicFile.LoadFromStream(stream, NbtCompression.AutoDetect);
			stream.Close();

			NbtCompound nbt = schematicFile.RootTag as NbtCompound;

			Width = nbt["Width"].IntValue;
			Height = nbt["Height"].IntValue;
			Depth = nbt["Length"].IntValue;

			byte[] blocks = nbt["Blocks"].ByteArrayValue;
			byte[] metadata = nbt["Data"].ByteArrayValue;

			Blocks = blocks;
			Metadata = new NibbleArray { Data = metadata };

			for (var y = 0; y < Height; y++)
			{
				for (var z = 0; z < Depth; z++)
				{
					for (var x = 0; x < Width; x++)
					{
						int index = GetBlockIndex(x, y, z);

						byte blockId = blocks[index];
						byte data = metadata[index];

						Func<int, byte, byte> dataConverter = (i, b) => b;
						if (Convert.ContainsKey(blockId))
						{
							dataConverter = Convert[blockId].Item2;
							blockId = (byte)Convert[blockId].Item1;
						}

						if (blockId > 255)
						{
							blockId = 41;
						}

						data = dataConverter(blockId, data);

						Blocks[index] = blockId;
						Metadata[index] = data;
					}
				}
			}
		}

		public int GetBlockIndex(int bx, int by, int bz)
		{
			if (InvertX) bx = Width - bx - 1;
			if (InvertZ) bz = Depth - bz - 1;

			return (by * Depth + bz) * Width + bx;
		}

		public bool InvertX { get; set; } = false;
		public bool InvertZ { get; set; } = false;

		public void Paste(OpenLevel level, BlockCoordinates position)
		{
			for (int x = 0; x < Width; x++)
			{
				for (int z = 0; z < Depth; z++)
				{
					for (int y = 0; y < Height; y++)
					{
						int index = GetBlockIndex(x, y, z);
						byte blockId = Blocks[index];
						byte meta = Metadata[index];

						level.SetBlock(position.X + x, position.Y + y, position.Z + z, blockId, meta, true, true, false);
					}
				}
			}
		}
	}
}
