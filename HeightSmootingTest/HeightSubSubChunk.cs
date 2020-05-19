namespace HeightSmootingTest
{
    public class HeightSubSubChunk
    {
        //01||02
        //03||04


        public HeightSubSubSubChunk[] Chunks = new HeightSubSubSubChunk[3];

        /// <summary>
        /// 4 BY 4 CONTAINER
        /// </summary>
        /// <param name="map"></param>
        public HeightSubSubChunk(int[,] map)
        {
            var scd1 = new int[1, 1];
            var scd2 = new int[1, 1];
            var scd3 = new int[1, 1];
            var scd4 = new int[1, 1];
            for (var x = 0; x < map.GetLength(0); x++)
            for (var z = 0; z < map.GetLength(1); z++)
                if (x >= 4 && z >= 4)
                    scd4[x, z] = map[x, z];
                else if (x < 4 && z >= 4)
                    scd3[x, z] = map[x, z];
                else if (x >= 4 && z < 4)
                    scd2[x, z] = map[x, z];
                else if (x <= 3 && z <= 3) scd1[x, z] = map[x, z];
            Chunks[0] = new HeightSubSubSubChunk(scd1);
            Chunks[1] = new HeightSubSubSubChunk(scd2);
            Chunks[2] = new HeightSubSubSubChunk(scd3);
            Chunks[3] = new HeightSubSubSubChunk(scd4);
        }
    }
}