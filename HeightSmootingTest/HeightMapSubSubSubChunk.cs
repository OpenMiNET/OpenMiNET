namespace HeightSmootingTest
{
   
    public class HeightSubSubSubChunk
    {
        public int[] v = new int[3];

        //XX||XX
        //XX||XX
        public HeightSubSubSubChunk(int[,] map)
        {
            v[0] = map[0, 0];
            v[1] = map[0, 1];
            v[2] = map[1, 0];
            v[3] = map[1, 1];
        }

        public int get(int x, int z)
        {
            if (x == 0 && z == 0) return v[0];
            if (x == 0 && z == 1) return v[1];
            if (x == 1 && z == 0) return v[2];
            if (x == 1 && z == 1) return v[3];
            return -1;
        }

        public void set(int x, int z, int val)
        {
            if (x == 0 && z == 0) v[0] = val;
            if (x == 0 && z == 1) v[1] = val;
            if (x == 1 && z == 0) v[2] = val;
            if (x == 1 && z == 1) v[3] = val;
        }

        public int getAverage()
        {
            return (v[0] + v[1] + v[2] + v[3]) / 4;
        }
    }
}