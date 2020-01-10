using System;
using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Worlds;

namespace OpenAPI.WorldGenerator.Generators.Structures
{
	public class Structure
	{
		protected readonly Random Rnd = new Random();
		public virtual string Name
		{
			get { return null; }
		}

		public virtual Block[] Blocks
		{
			get { return null; }
		}

		public virtual int MaxHeight { get { return 0; } }

		public virtual void Create(int[] blocks, int[] metadata, int x, int y, int z)
		{
			if (blocks[OverworldGenerator.GetIndex(x, y + MaxHeight, z)] == (byte) 0)
			{
				foreach (Block b in Blocks)
				{
					//chunk.SetBlock(x + b.Coordinates.X, y + b.Coordinates.Y, z + b.Coordinates.Z, b.Id);
					//chunk.SetMetadata(x + b.Coordinates.X, y + b.Coordinates.Y, z + b.Coordinates.Z, b.Metadata);
					blocks[OverworldGenerator.GetIndex(x + b.Coordinates.X, y + b.Coordinates.Y, z + b.Coordinates.Z)] = b.Id;
					metadata[OverworldGenerator.GetIndex(x + b.Coordinates.X, y + b.Coordinates.Y, z + b.Coordinates.Z)] = b.Id;
				}
			}
		}

		public virtual void Create(Level level, int x, int y, int z)
		{
			if (level.IsAir(new BlockCoordinates(x, y + MaxHeight, z)))
			{
				foreach (Block b in Blocks)
				{
					Block clone = BlockFactory.GetBlockById(b.Id);
					clone.Metadata = b.Metadata;
					clone.Coordinates = new BlockCoordinates(x + b.Coordinates.X, y + b.Coordinates.Y, z + b.Coordinates.Z);
					
					level.SetBlock(clone);
				}
			}
		}
	}
}
