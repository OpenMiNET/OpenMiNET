using System;
using MiNET.Worlds;
using MiNET.Worlds.Structures;
using OpenAPI.Utils;
using OpenAPI.WorldGenerator.Generators.Structures;
using OpenAPI.WorldGenerator.Utils.Noise;
using Biome = OpenAPI.WorldGenerator.Utils.Biome;
using Structure = OpenAPI.WorldGenerator.Generators.Structures.Structure;

namespace OpenAPI.WorldGenerator.Generators.Decorators
{
    public class FoliageDecorator : ChunkDecorator
	{
		public override void Decorate(int chunkX, int chunkZ, int[] blocks, int[] metadata, Biome biome, float[] thresholdMap, int x, int y, int z, bool surface,
			bool isBelowMaxHeight)
		{
			try
			{
				var currentTemperature = biome.Temperature;
				if (y > 64)
				{
					int distanceToSeaLevel = y - 64;
					currentTemperature = biome.Temperature - (0.00166667f*distanceToSeaLevel);
				}

				int rx = chunkX*16 + x;
				int rz = chunkZ*16 + z;

				bool generated = false;
				if (surface && y >= OverworldGenerator.WaterLevel)
				{
					var noise = Simplex.Noise(rx, rz, Math.Min(biome.Downfall * 0.32f, 0.03f), 0.5, true);
					if (x >= 3 && x <= 13 && z >= 3 && z <= 13)
					{
						Structure tree = null;
						if (biome.Downfall <= 0f && biome.Temperature >= 2f)
						{
							if (GetRandom(32) == 16)
							{
								var randValue = GetRandom(18);
								if (randValue >= 0 && randValue <= 2) //3 tall cactus
								{
									tree = new CactusStructure(3);
								}
								else if (randValue > 2 && randValue <= 5) // 2 tall cactus
								{
									tree = new CactusStructure(2);
								}
								else if (randValue > 5 && randValue <= 11) // 1 tall cactus
								{
									tree = new CactusStructure(1);
								}
							}
						}

						if (tree == null && biome.Downfall >= 0 && (noise > (0.5f + (y/512f))))
						{
							if (currentTemperature >= 1f && biome.Downfall >= 0.4f)
							{
								if (GetRandom(8) == 4)
								{
									tree = new LargeJungleTree();
								}
								else
								{
									tree = new SmallJungleTree();
								}
							}
							/*	else if (currentTemperature >= 0.7F && biome.Downfall >= 0.2f)
								{
									tree = new OakTree(true);
								}*/
							else if (currentTemperature >= 0.7F && biome.Downfall < 0.2f)
							{
								tree = new AcaciaTree();
							}
							else if (currentTemperature > 0.25f && biome.Downfall > 0f)
							{
								if (biome.Name.Contains("Birch") || GetRandom(16) == 8)
								{
									tree = new BirchTree();
								}
								else
								{
									tree = new OakTree();
								}
							}
							else if (currentTemperature <= 0.25f && biome.Downfall > 0f)
							{
								tree = new PineTree();
							}
						}

						if (tree != null)
						{
							if (y + 1 < 254)
							{
								tree.Create(blocks, metadata, x, y + 1, z);
							}
							generated = true;
						}
					}

					if (!generated)
					{
						if (noise > 0.5) //Threshold 1
						{
							/*if (currentTemperature > 0.3f && currentTemperature < 1.5f && biome.Downfall >= 0.85f)
							{
								column.SetBlock(x, y + 1, z, 18); //Leaves
								column.SetMetadata(x, y + 1, z, 3); //Jungle Leaves
							}
							else*/
							if (currentTemperature > 0.3f && currentTemperature < 1.5f && biome.Downfall > 0)
							{
								var blockBeneath = blocks[OverworldGenerator.GetIndex(x, y, z)];// column.GetBlock(x, y, z);

								var sugarPosibility = GetRandom(18);
								if ( /*sugarPosibility <= 11*/ noise > 0.75f &&
								                               (blockBeneath == 3 || blockBeneath == 2 || blockBeneath == 12) &&
								                               IsValidSugarCaneLocation(blocks, x, y, z))
								{
									int height = 1;
									if (sugarPosibility <= 2)
									{
										height = 3;
									}
									else if (sugarPosibility <= 5)
									{
										height = 2;
									}

									//var growth = Rnd.Next(0x1, 0x15);
									for (int mY = y + 1; mY < y + 1 + height; mY++)
									{
										//column.SetBlock(x, mY, z, 83); //SugarCane
										blocks[OverworldGenerator.GetIndex(x, mY, z)] = 83;
										
										if (mY == y + 1 + height)
										{
											metadata[OverworldGenerator.GetIndex(x, mY, z)] = (byte) Rnd.Next(0, 15);
											//column.SetMetadata(x, mY, z, (byte) Rnd.Next(0, 15));
										}
										else
										{
											metadata[OverworldGenerator.GetIndex(x, mY, z)] = (byte) 0;
											//column.SetMetadata(x, mY, z, 0);
										}
									}
								}
								else if (noise > 0.8 && blockBeneath == 3 || blockBeneath == 2) //If above 0.8, we generate flowers :)
								{
									if (Simplex.Noise(rx, rz, 0.5f, 0.5f, true) > 0.5)
									{
										blocks[OverworldGenerator.GetIndex(x, y + 1, z)] = 38;
										metadata[OverworldGenerator.GetIndex(x, y + 1, z)] = (byte) GetRandom(8);
										//column.SetBlock(x, y + 1, z, 38); //Poppy
										//column.SetMetadata(x, y + 1, z, (byte) GetRandom(8));
									}
									else
									{
										blocks[OverworldGenerator.GetIndex(x, y + 1, z)] = 37;
									//	column.SetBlock(x, y + 1, z, 37); //Dandelion
									}
								}
								else if (blockBeneath == 3 || blockBeneath == 2)
								{
									blocks[OverworldGenerator.GetIndex(x, y + 1, z)] = 31;
									metadata[OverworldGenerator.GetIndex(x, y + 1, z)] = (byte) 1;
									//column.SetBlock(x, y + 1, z, 31); //Grass
									//column.SetMetadata(x, y + 1, z, 1);
								}
							}

						}
					}
				}
			}
			catch (Exception e)
			{
				
			}
		}

		private bool IsValidSugarCaneLocation(int[] blocks, int x, int y, int z)
		{
			if (y - 1 <= 0) return false;
			if (x - 1 >= 0 && x + 1 < 16)
			{
				if (z - 1 >= 0 && z + 1 < 16)
				{
					if (blocks[OverworldGenerator.GetIndex(x + 1, y, z)] == 8 
						|| blocks[OverworldGenerator.GetIndex(x - 1, y, z)] == 8 
						          || blocks[OverworldGenerator.GetIndex(x, y, z + 1)] == 8 
						          || blocks[OverworldGenerator.GetIndex(x, y, z - 1)] == 8)
					{
						return true;
					}
				}
			}
			return false;
		}

		private int GetRandom(int max)
		{
			return Rnd.Next(max);
		}

		private FastRandom Rnd { get; set; }
		private SimplexOctaveGenerator Simplex { get; set; }
		protected override void InitSeed(int seed)
		{
			Simplex = new SimplexOctaveGenerator(seed, 1);
			Simplex.SetScale(1.5);

			Rnd = new FastRandom(seed);
		}
	}
}