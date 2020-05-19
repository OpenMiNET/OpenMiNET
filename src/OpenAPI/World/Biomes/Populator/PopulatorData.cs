using System;
using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Worlds;

namespace OpenAPI.World.Populator
{
    public class PopulatorData
    {
        public Block block;
        public int clusterCount;
        public int clusterSize;
        public int minHeight;
        public int maxHeight;

        public bool spawn(Level level, Random rand, int x, int y, int z)
        {
            double piScaled = rand.NextDouble() * (float) Math.PI;
            double scaleMaxX = (double) ((float) (x + 8) + Math.Sin(piScaled) * (float) clusterSize / 8.0F);
            double scaleMinX = (double) ((float) (x + 8) - Math.Sin(piScaled) * (float) clusterSize / 8.0F);
            double scaleMaxZ = (double) ((float) (z + 8) + Math.Cos(piScaled) * (float) clusterSize / 8.0F);
            double scaleMinZ = (double) ((float) (z + 8) - Math.Cos(piScaled) * (float) clusterSize / 8.0F);
            double scaleMaxY = (double) (y + rand.Next(3) - 2);
            double scaleMinY = (double) (y + rand.Next(3) - 2);

            for (int i = 0; i < clusterSize; ++i)
            {
                float sizeIncr = (float) i / (float) clusterSize;
                double scaleX = scaleMaxX + (scaleMinX - scaleMaxX) * (double) sizeIncr;
                double scaleY = scaleMaxY + (scaleMinY - scaleMaxY) * (double) sizeIncr;
                double scaleZ = scaleMaxZ + (scaleMinZ - scaleMaxZ) * (double) sizeIncr;
                double randSizeOffset = rand.NextDouble() * (double) clusterSize / 16.0D;
                double randVec1 = (double) (Math.Sin((float) Math.PI * sizeIncr) + 1.0F) * randSizeOffset + 1.0D;
                double randVec2 = (double) (Math.Sin((float) Math.PI * sizeIncr) + 1.0F) * randSizeOffset + 1.0D;
                int minX = (int) Math.Floor(scaleX - randVec1 / 2.0D);
                int minY = (int) Math.Floor(scaleY - randVec2 / 2.0D);
                int minZ = (int) Math.Floor(scaleZ - randVec1 / 2.0D);
                int maxX = (int) Math.Floor(scaleX + randVec1 / 2.0D);
                int maxY = (int) Math.Floor(scaleY + randVec2 / 2.0D);
                int maxZ = (int) Math.Floor(scaleZ + randVec1 / 2.0D);

                for (int xSeg = minX; xSeg <= maxX; ++xSeg)
                {
                    double xVal = ((double) xSeg + 0.5D - scaleX) / (randVec1 / 2.0D);

                    if (xVal * xVal < 1.0D)
                    {
                        for (int ySeg = minY; ySeg <= maxY; ++ySeg)
                        {
                            double yVal = ((double) ySeg + 0.5D - scaleY) / (randVec2 / 2.0D);

                            if (xVal * xVal + yVal * yVal < 1.0D)
                            {
                                for (int zSeg = minZ; zSeg <= maxZ; ++zSeg)
                                {
                                    double zVal = ((double) zSeg + 0.5D - scaleZ) / (randVec1 / 2.0D);

                                    if (xVal * xVal + yVal * yVal + zVal * zVal < 1.0D)
                                    {
                                        if (level.GetBlock(xSeg, ySeg, zSeg).Id == new Stone().Id)
                                        {
                                            block.Coordinates = new BlockCoordinates(xSeg, ySeg, zSeg);
                                            level.SetBlock(block);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}