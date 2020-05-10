namespace HeightSmootingTest
{
    public class HeightChunk
    {
        public HeightSubChunk[] Chunks = new HeightSubChunk[3];

        public HeightChunk(int[,] map)
        {
            var scd1 = new int[7, 7];
            var scd2 = new int[7, 7];
            var scd3 = new int[7, 7];
            var scd4 = new int[7, 7];
            for (var x = 0; x < map.GetLength(0); x++)
            for (var z = 0; z < map.GetLength(1); z++)
                if (x >= 8 && z >= 8)
                    scd4[x, z] = map[x, z];
                else if (x < 8 && z >= 8)
                    scd3[x, z] = map[x, z];
                else if (x >= 8 && z < 8)
                    scd2[x, z] = map[x, z];
                else if (x <= 7 && z <= 7) scd1[x, z] = map[x, z];
            Chunks[0] = new HeightSubChunk(scd1);
            Chunks[1] = new HeightSubChunk(scd2);
            Chunks[2] = new HeightSubChunk(scd3);
            Chunks[3] = new HeightSubChunk(scd4);
        }
    }
}