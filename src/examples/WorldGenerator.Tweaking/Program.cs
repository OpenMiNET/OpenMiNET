using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibNoise;
using MiNET.Utils;
using MiNET.Worlds;
using Newtonsoft.Json;
using OpenAPI.WorldGenerator.Generators;
using OpenAPI.WorldGenerator.Utils;
using OpenAPI.WorldGenerator.Utils.Noise;
using Biome = OpenAPI.WorldGenerator.Utils.Biome;
using BiomeUtils = OpenAPI.WorldGenerator.Utils.BiomeUtils;

namespace WorldGenerator.Tweaking
{
    class Program
    {
        private static ConcurrentQueue<ChunkColumn> Finished = new ConcurrentQueue<ChunkColumn>();
        static void Main(string[] args)
        {
            int chunks = 64;
            int width = chunks * 16;
            int height = chunks * 16;

            OverworldGeneratorV2 gen = new OverworldGeneratorV2();
            gen.ApplyBlocks = true;
            
            bool done = false;
          //  ChunkColumn[] generatedChunks = new ChunkColumn[chunks * chunks];
            ConcurrentQueue<ChunkCoordinates> chunkGeneratorQueue = new ConcurrentQueue<ChunkCoordinates>();
            
            long average = 0;
            long min = long.MaxValue;
            long max = long.MinValue;

            int chunskGenerated = 0;
            Thread[] threads = new Thread[Environment.ProcessorCount -1];
            for (int t = 1; t < threads.Length; t++)
            {
                threads[t] = new Thread(() =>
                {
                    Stopwatch timing = new Stopwatch();
                    while (true)
                    {
                        if (chunkGeneratorQueue.TryDequeue(out var coords))
                        {
                            timing.Restart();
                            
                            ChunkColumn column = gen.GenerateChunkColumn(coords);
                           // generatedChunks[(coords.X * chunks) + coords.Z] = column;
                            Finished.Enqueue(column);
                            chunskGenerated++;
                            
                            timing.Stop();
                            
                            average += timing.ElapsedMilliseconds;
                            if (timing.ElapsedMilliseconds < min)
                                min = timing.ElapsedMilliseconds;
                            
                            if (timing.ElapsedMilliseconds > max)
                                max = timing.ElapsedMilliseconds;
                        }
                        else
                        {
                            break;
                        }
                    }
                });
            }
            
            threads[0] = new Thread(() => { GenerateBiomeMap(chunks); });

            for (int x = 0; x < chunks; x++)
            {
                for(int z = 0; z < chunks; z++)
                {
                    chunkGeneratorQueue.Enqueue(new ChunkCoordinates(x, z));
                }
            }
            
            Stopwatch timer = Stopwatch.StartNew();
            foreach (var thread in threads)
            {
                thread.Start();
            }

            int threadsAlive = 0;
            do
            {
                threadsAlive = threads.Count(x => x.IsAlive);
                
                Console.Clear();
                
                Console.WriteLine($"Threads: {threadsAlive} Queued: {chunkGeneratorQueue.Count} Generated: {chunskGenerated} Avg: {average / Math.Max(1, chunskGenerated)}ms Min: {min}ms Max: {max}ms");
                Console.WriteLine($"Processed: {Imaged} Remaining: {Finished.Count}");
                
                Thread.Sleep(100);
            } while (threadsAlive > 0);

            timer.Stop();
            
            Console.Clear();
            
            Console.WriteLine($"Generating {chunks * chunks} chunks took: {timer.Elapsed}");
            Console.WriteLine($"Min Height: {gen.MinHeight} Max Height: {gen.MaxHeight}");
        }

        public static int Imaged { get; set; } = 0;

        private static void GenerateBiomeMap(int chunks)
        {
            int finished = 0;
            Bitmap bitmap = new Bitmap(chunks * 16, chunks * 16);
            Bitmap heightmap = new Bitmap(chunks * 16, chunks * 16);
            Bitmap chunkHeight = new Bitmap(chunks * 16, chunks * 16);

            while (finished < chunks * chunks)
            {
                if (Finished.TryDequeue(out ChunkColumn column))
                {
                    for (int cx = 0; cx < 16; cx++)
                    {
                        var rx = (column.X * 16) + cx;
                        for (int cz = 0; cz < 16; cz++)
                        {
                            var rz = (column.Z * 16) + cz;

                            var biome = BiomeUtils.GetBiomeById(column.GetBiome(cx, cz));
                            var temp = (int) Math.Max(0,
                                Math.Min(255, (255 * MathUtils.ConvertRange(-1f, 2f, 0f, 1f, biome.Temperature))));
                            var humid = (int) Math.Max(32,
                                Math.Min(255, (255 * biome.Downfall)));

                            bitmap.SetPixel(rx, rz, Color.FromArgb(humid, temp, 0, 255 - temp));

                            int height = column.GetHeight(cx, cz);

                            chunkHeight.SetPixel(rx, rz, Color.FromArgb(height, height, height));

                            height = (int) Math.Max(0,
                                Math.Min(255,
                                    (255 * MathUtils.ConvertRange(-2f, 2f, 0f, 1f,
                                         ((biome.MinHeight + biome.MaxHeight) / 2f)))));

                            heightmap.SetPixel(rx, rz, Color.FromArgb(height, height, height));
                        }
                    }

                    Imaged++;
                    finished++;
                }
                else
                {
                    Thread.Sleep(50);
                }
            }

            bitmap.Save("heatmap.png", ImageFormat.Png);
            heightmap.Save("height.png", ImageFormat.Png);
            chunkHeight.Save("chunkHeight.png", ImageFormat.Png);
        }

        private static Task GenerateHeightmap(ChunkColumn[] columns, int chunks, bool chunkHeight)
        {
            return Task.Run(() =>
            {
                Bitmap bitmap = new Bitmap(chunks * 16, chunks * 16);
                for (int x = 0; x < chunks; x++)
                {
                    for (int z = 0; z < chunks; z++)
                    {
                        ChunkColumn column = columns[(x * chunks) + z];
                        for (int cx = 0; cx < 16; cx++)
                        {
                            var rx = (x * 16) + cx;
                            for (int cz = 0; cz < 16; cz++)
                            {
                                var rz = (z * 16) + cz;

                              //  var height = column.GetHeight(cx, cz);
                              //  var temp = (int) Math.Max((byte)0,
                               //     Math.Min((byte)255, height));

                               var height = 0;

                               if (!chunkHeight)
                               {
                                   height = column.GetHeight(cx, cz);
                               }
                               else
                               {
                                   var biome = BiomeUtils.GetBiomeById(column.GetBiome(cx, cz));
                                   height = (int) Math.Max(0,
                                       Math.Min(255, (255 * MathUtils.ConvertRange(-2f, 2f, 0f, 1f, ((biome.MinHeight + biome.MaxHeight) / 2f)))));
                               }
                               
                                bitmap.SetPixel(rx, rz, Color.FromArgb(height, height, height));
                            }
                        }
                    }
                }

                bitmap.Save(chunkHeight ? "chunkHeight.png" : "height.png", ImageFormat.Png);
            });
        }
    }
}