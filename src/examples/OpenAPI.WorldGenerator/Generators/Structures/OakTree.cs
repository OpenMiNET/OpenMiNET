using System.Numerics;
using MiNET.Worlds;

namespace OpenAPI.WorldGenerator.Generators.Structures
{
	public class OakTree : TreeStructure
	{
		private bool IsWet { get; }
		public OakTree(bool isWet = false)
		{
			IsWet = isWet;
		}

		public override string Name
		{
			get { return "OakTree"; }
		}

		public override int MaxHeight
		{
			get { return 7; }
		}

		private readonly int _leafRadius = 2;
		public override void Create(int[] blocks, int[] metadata, int x, int y, int z)
		{
			var block = blocks[OverworldGenerator.GetIndex(x, y - 1, z)];
			if (block != 2 && block != 3) return;

			var location = new Vector3(x, y, z);
			if (!ValidLocation(location, _leafRadius)) return;

			int height = Rnd.Next(4, 5);
			GenerateColumn(blocks, metadata, location, height, 17, 0);
			Vector3 leafLocation = location + new Vector3(0, height, 0);
			GenerateVanillaLeaves(blocks, metadata, leafLocation, _leafRadius, 18, 0);
		}

		public override void Create(Level level, int x, int y, int z)
		{
			var location = new Vector3(x, y, z);

			int height = Rnd.Next(4, 5);
			GenerateColumn(level, location, height, 17, 0);
			Vector3 leafLocation = location + new Vector3(0, height, 0);
			GenerateVanillaLeaves(level, leafLocation, _leafRadius, 18, 0);
		}
	}
}
