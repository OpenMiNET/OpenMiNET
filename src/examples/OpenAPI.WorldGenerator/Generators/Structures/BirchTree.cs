using System.Numerics;
using MiNET.Worlds;

namespace OpenAPI.WorldGenerator.Generators.Structures
{
    class BirchTree : TreeStructure
	{
        public override string Name
        {
            get { return "BirchTree"; }
        }

        public override int MaxHeight
        {
            get { return 7; }
        }

		private const int LeafRadius = 2;
		public override void Create(int[] blocks, int[] metadata, int x, int y, int z)
	    {
			var block = blocks[OverworldGenerator.GetIndex(x, y - 1, z)];
			if (block != 2 && block != 3) return;

			var location = new Vector3(x, y, z);
			if (!ValidLocation(location, LeafRadius)) return;

			int height = Rnd.Next(4, MaxHeight);
			GenerateColumn(blocks, metadata, location, height, 17, 2);
			Vector3 leafLocation = location + new Vector3(0, height, 0);
			GenerateVanillaLeaves(blocks, metadata, leafLocation, LeafRadius, 18, 2);
		}

		public override void Create(Level level, int x, int y, int z)
		{
			var location = new Vector3(x, y, z);

			int height = Rnd.Next(4, MaxHeight);
			GenerateColumn(level, location, height, 17, 2);
			Vector3 leafLocation = location + new Vector3(0, height, 0);
			GenerateVanillaLeaves(level, leafLocation, LeafRadius, 18, 2);
		}
	}
}
