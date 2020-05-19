namespace HeightSmootingTest
{
    public class HeightSubChunk
    {
        //XX|XX|XX|XX|
        //XX|XX|XX|XX|
        //XX|XX|XX|XX|
        //XX|XX|XX|XX|
        public HeightSubSubChunk[] Chunks = new HeightSubSubChunk[3];


        /// <summary>
        ///
        /// 8 BY 8 CONTAINER
        /// </summary>
        /// <param name="map">8 bits</param>
        public HeightSubChunk(int[,] map)
        {
            var scd1 = new int[3, 3];
            var scd2 = new int[3, 3];
            var scd3 = new int[3, 3];
            var scd4 = new int[3, 3];
            for (var x = 0; x < map.GetLength(0); x++)
            for (var z = 0; z < map.GetLength(1); z++)
                if (x >= 4 && z >= 4)
                    scd4[x, z] = map[x, z];
                else if (x < 4 && z >= 4)
                    scd3[x, z] = map[x, z];
                else if (x >= 4 && z < 4)
                    scd2[x, z] = map[x, z];
                else if (x <= 3 && z <= 3) scd1[x, z] = map[x, z];
            Chunks[0] = new HeightSubSubChunk(scd1);
            Chunks[1] = new HeightSubSubChunk(scd2);
            Chunks[2] = new HeightSubSubChunk(scd3);
            Chunks[3] = new HeightSubSubChunk(scd4);
        }
        

    }

    //4 8X8
    
}