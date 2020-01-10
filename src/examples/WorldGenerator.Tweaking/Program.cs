using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using LibNoise;
using Newtonsoft.Json;
using OpenAPI.WorldGenerator.Generators;
using OpenAPI.WorldGenerator.Utils;

namespace WorldGenerator.Tweaking
{
    class Program
    {
        static void Main(string[] args)
        {
            var generatorPreset = JsonConvert.DeserializeObject<WorldGeneratorPreset>("{\"coordinateScale\":175.0,\"heightScale\":75.0,\"lowerLimitScale\":512.0,\"upperLimitScale\":512.0,\"depthNoiseScaleX\":200.0,\"depthNoiseScaleZ\":200.0,\"depthNoiseScaleExponent\":0.5,\"mainNoiseScaleX\":165.0,\"mainNoiseScaleY\":106.61267,\"mainNoiseScaleZ\":165.0,\"baseSize\":8.267606,\"stretchY\":13.387607,\"biomeDepthWeight\":1.2,\"biomeDepthOffset\":0.2,\"biomeScaleWeight\":3.4084506,\"biomeScaleOffset\":0.0,\"seaLevel\":63,\"useCaves\":true,\"useDungeons\":true,\"dungeonChance\":7,\"useStrongholds\":true,\"useVillages\":true,\"useMineShafts\":true,\"useTemples\":true,\"useMonuments\":true,\"useRavines\":true,\"useWaterLakes\":true,\"waterLakeChance\":49,\"useLavaLakes\":true,\"lavaLakeChance\":80,\"useLavaOceans\":false,\"fixedBiome\":-1,\"biomeSize\":8,\"riverSize\":5,\"dirtSize\":33,\"dirtCount\":10,\"dirtMinHeight\":0,\"dirtMaxHeight\":256,\"gravelSize\":33,\"gravelCount\":8,\"gravelMinHeight\":0,\"gravelMaxHeight\":256,\"graniteSize\":33,\"graniteCount\":10,\"graniteMinHeight\":0,\"graniteMaxHeight\":80,\"dioriteSize\":33,\"dioriteCount\":10,\"dioriteMinHeight\":0,\"dioriteMaxHeight\":80,\"andesiteSize\":33,\"andesiteCount\":10,\"andesiteMinHeight\":0,\"andesiteMaxHeight\":80,\"coalSize\":17,\"coalCount\":20,\"coalMinHeight\":0,\"coalMaxHeight\":128,\"ironSize\":9,\"ironCount\":20,\"ironMinHeight\":0,\"ironMaxHeight\":64,\"goldSize\":9,\"goldCount\":2,\"goldMinHeight\":0,\"goldMaxHeight\":32,\"redstoneSize\":8,\"redstoneCount\":8,\"redstoneMinHeight\":0,\"redstoneMaxHeight\":16,\"diamondSize\":8,\"diamondCount\":1,\"diamondMinHeight\":0,\"diamondMaxHeight\":16,\"lapisSize\":7,\"lapisCount\":1,\"lapisCenterHeight\":16,\"lapisSpread\":16}");
            NoiseProvider noiseProvider = new NoiseProvider(generatorPreset, "test-world".GetHashCode());

            //OverworldGenerator overworldGenerator = new OverworldGenerator();
            
            var tasks = new Task[]
            {
                GenerateAndSave(noiseProvider.TempNoise, "temperature"),
                GenerateAndSave(noiseProvider.RainNoise, "humidity"),
                GenerateAndSave(noiseProvider.BaseHeightNoise, "baseheight"),
                GenerateAndSave(noiseProvider.TerrainNoise, "terrain")
            };
            
            Task.WaitAll(tasks);
        }

        private static Task GenerateAndSave(IModule2D noise, string name)
        {
            return Task.Run(() =>
            {
                Stopwatch sw = Stopwatch.StartNew();
                Console.WriteLine($"Generating {name}...");
                
                Bitmap bmp = new Bitmap(16 * 128, 16 * 128);

                var halfWidth = (bmp.Width / 2);
                var halfHeight = (bmp.Height / 2);
                for (int x = -halfWidth; x < halfWidth; x++)
                {
                    for (int y = -halfHeight; y < halfHeight; y++)
                    {
                        int r = 255, g = 0, b = 0;

                        if (x % 16 != 0 && y % 16 != 0)
                        {
                            var temperature = noise.GetValue(x, y);
                            //  var rainfall = _rainNoise.GetValue(x, y);

                            int color = (int) Math.Max(0,
                                Math.Min(255, (255 * MathUtils.ConvertRange(-1f, 1f, 0f, 1f, -temperature))));
                            r = color;
                            g = color;
                            b = color;
                        }

                        bmp.SetPixel(x + halfWidth, y + halfHeight, Color.FromArgb(r, g, b));
                    }
                }

                bmp.Save($"{name}.png", ImageFormat.Png);

                Console.WriteLine($"Generated {name} in {sw.ElapsedMilliseconds}ms");
                sw.Stop();
            });
        }
    }
}