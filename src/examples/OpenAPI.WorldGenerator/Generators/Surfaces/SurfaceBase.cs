using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Worlds;
using OpenAPI.Utils;
using OpenAPI.WorldGenerator.Generators.Biomes;
using OpenAPI.WorldGenerator.Generators.Biomes.Config;
using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Surfaces
{
    public class SurfaceBase
    {
        public Block ShadowStoneBlock;
        public Block ShadowDesertBlock;
        protected Block TopBlock;
        protected Block FillerBlock;
        protected Block CliffStoneBlock;
        protected Block CliffCobbleBlock;
        protected BiomeConfig BiomeConfig;

        private FastRandom _rnd = new FastRandom();
        
        public SurfaceBase(BiomeConfig config, Block top, Block filler)
        {
            BiomeConfig = config;
            TopBlock = top;
            FillerBlock = filler;
            
            CliffStoneBlock = new Stone();
            CliffCobbleBlock = new Cobblestone();
        }

        public void PaintTerrain(ChunkColumn column, int i, int j, int x, int z, int depth,
            OverworldGeneratorV2 generator, float[] noise, float river, BiomeBase[] biomes)
        {
            float c = TerrainBase.CalcCliff(x, z, noise);
            bool cliff = c > 1.4f;

            for (int k = 255; k > -1; k--) {
                Block b = column.GetBlockObject(x, k, z);
                if (b is Air) {
                    depth = -1;
                }
                else if (b is Stone) {
                    depth++;

                    if (cliff) {
                        if (depth > -1 && depth < 2) {
                            if (_rnd.Next(3) == 0) {

                                column.SetBlock(x, k, z, CliffCobbleBlock);
                            }
                            else {

                                column.SetBlock(x, k, z, CliffStoneBlock);
                            }
                        }
                        else if (depth < 10) {
                            column.SetBlock(x, k, z, CliffStoneBlock);
                        }
                    }
                    else {
                        if (depth == 0 && k > 61) {
                            column.SetBlock(x, k, z, TopBlock);
                        }
                        else if (depth < 4) {
                            column.SetBlock(x, k, z, FillerBlock);
                        }
                    }
                }
            }
        }
    }
}