using System;
using MiNET.Blocks;
using MiNET.Worlds;

namespace OpenAPI.World.Populator
{
    public class OrePopulator : Populator
    {
        public int replaceId = new Stone().Id;
        public new void Populate(Level level,ChunkColumn c)
        {

            int chunkX = c.X;
            int chunkZ = c.Z;
            int sx = chunkX << 4;
            int ex = sx + 15;
            int sz = chunkZ << 4;
            int ez = sz + 15;
            var NukkitMath = new Random();
            foreach (PopulatorData type in Data) {
                for (int i = 0; i < type.clusterCount; i++) {
                    int x = NukkitMath.Next(sx, ex);
                    int z = NukkitMath.Next( sz, ez);
                    int y = NukkitMath.Next( type.minHeight, type.maxHeight);
                    if (c.GetBlockId(x, y, z) != replaceId) {
                        continue;
                    }
                    type.spawn(level, NukkitMath, x, y, z);
                }
            }
        }
    }
}