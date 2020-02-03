using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OpenAPI.WorldGenerator.Generators.Biomes
{
     public class SmoothingSearchStatus {

        private int _upperLeftFinding = 0;
        private int _upperRightFinding = 3;
        private int _lowerLeftFinding = 1;
        private int _lowerRightFinding = 4;
        private int[] _quadrantBiome = new int[4];
        private float[] _quadrantBiomeWeighting = new float[4];
        public int[] Biomes = new int[OverworldGeneratorV2.MAX_BIOMES];
        private bool _absent = false;
        private bool _notHunted;
        private int[] _findings = new int[3 * 3];
        // weightings is part of a system to generate some variability in repaired chunks weighting is
        // based on how long the search went on (so quasipsuedorandom, based on direction plus distance
        private float[] _weightings = new float[3 * 3];
        private bool[] _desired;
        private int _arraySize;
        private int[] _pattern;
        private int _biomeCount;

        public SmoothingSearchStatus(bool[] desired) {
            this._desired = desired;
            
            Biomes = new int[OverworldGeneratorV2.MAX_BIOMES];
            Array.Fill(Biomes, -1);
        }

        public int Size() {
            return 3;
        }

        public void Hunt(int[] biomeNeighborhood) {
            // 0,0 in the chunk is 9,9 int the array ; 8,8 is 10,10 and is treated as the center
            Clear();
            int oldArraySize = _arraySize;
            _arraySize = (int) Math.Sqrt(biomeNeighborhood.Length);
            if (_arraySize * _arraySize != biomeNeighborhood.Length) {
                throw new Exception("non-square array");
            }

            if (_arraySize != oldArraySize) {
                _pattern = new CircularSearchCreator().Pattern(_arraySize / 2f - 1, _arraySize);
            }

            for (int xOffset = -1; xOffset <= 1; xOffset++) {
                for (int zOffset = -1; zOffset <= 1; zOffset++) {
                    Search(xOffset, zOffset, biomeNeighborhood);
                }
            }
            // calling a routine because it gets too indented otherwise
            SmoothBiomes();
        }

        public void Search(int xOffset, int zOffset, int[] biomeNeighborhood) {
            int offset = xOffset * _arraySize + zOffset;
            int location = (xOffset + 1) * Size() + zOffset + 1;
            // set to failed search, which sticks if nothing is found
            _findings[location] = -1;
            _weightings[location] = 2f;
            for (int i = 0; i < _pattern.Length; i++) {
                int biome = biomeNeighborhood[_pattern[i] + offset];

                if (biome < 0 || biome >= _desired.Length - 1)
                {
                    Debugger.Break();
                }
                
                if (_desired[biome]) {
                    _findings[location] = biome;
                    _weightings[location] = (float) MathF.Sqrt(_pattern.Length) - (float) MathF.Sqrt(i) + 2f;
                    break;
                }
            }
        }

        public void SmoothBiomes() {
            // more sophisticated version offsets into findings and biomes upperleft
            SmoothQuadrant(BiomeIndex(0, 0), _upperLeftFinding);
            SmoothQuadrant(BiomeIndex(8, 0), _upperRightFinding);
            SmoothQuadrant(BiomeIndex(0, 8), _lowerLeftFinding);
            SmoothQuadrant(BiomeIndex(8, 8), _lowerRightFinding);
        }

        public void SmoothQuadrant(int biomesOffset, int findingsOffset) {
            int upperLeft = _findings[_upperLeftFinding + findingsOffset];
            int upperRight = _findings[_upperRightFinding + findingsOffset];
            int lowerLeft = _findings[_lowerLeftFinding + findingsOffset];
            int lowerRight = _findings[_lowerRightFinding + findingsOffset];
            // check for uniformity
            if ((upperLeft == upperRight) && (upperLeft == lowerLeft) && (upperLeft == lowerRight)) {
                // everythings the same; uniform fill;
                for (int x = 0; x < 8; x++) {
                    for (int z = 0; z < 8; z++) {
                        Biomes[BiomeIndex(x, z) + biomesOffset] = upperLeft;
                    }
                }
                return;
            }
            // not all the same; we have to work;
            _biomeCount = 0;
            AddBiome(upperLeft);
            AddBiome(upperRight);
            AddBiome(lowerLeft);
            AddBiome(lowerRight);
            for (int x = 0; x < 8; x++) {
                for (int z = 0; z < 8; z++) {
                    AddBiome(lowerRight);
                    for (int i = 0; i < 4; i++) {
                        _quadrantBiomeWeighting[i] = 0;
                    }
                    // weighting strategy: weights go down as you move away from the corner.
                    // they go to 0 on the far edges so only the points on the edge have effects there
                    // for continuity with the next quadrant
                    AddWeight(upperLeft, _weightings[_upperLeftFinding + findingsOffset] * (7 - x) * (7 - z));
                    AddWeight(upperRight, _weightings[_upperRightFinding + findingsOffset] * x * (7 - z));
                    AddWeight(lowerLeft, _weightings[_lowerLeftFinding + findingsOffset] * (7 - x) * z);
                    AddWeight(lowerRight, _weightings[_lowerRightFinding + findingsOffset] * x * z);
                    Biomes[BiomeIndex(x, z) + biomesOffset] = PreferredBiome();
                }
            }
        }

        public void AddBiome(int biome) {
            for (int i = 0; i < _biomeCount; i++) {
                if (biome == _quadrantBiome[i]) {
                    return;
                }
            }
            // not there, add
            _quadrantBiome[_biomeCount++] = biome;
        }

        public void AddWeight(int biome, float weight) {
            for (int i = 0; i < _biomeCount; i++) {
                if (biome == _quadrantBiome[i]) {
                    _quadrantBiomeWeighting[i] += weight;
                    return;
                }
            }
        }

        public int PreferredBiome() {
            float bestWeight = 0;
            int result = -2;
            for (int i = 0; i < _biomeCount; i++) {
                if (_quadrantBiomeWeighting[i] > bestWeight) {
                    bestWeight = _quadrantBiomeWeighting[i];
                    result = _quadrantBiome[i];
                }
            }
            return result;
        }

        public int BiomeIndex(int x, int z) {
            return x * 16 + z;
        }

        public void Clear() {
            Array.Fill(_findings, -1);
          //  Arrays.fill(findings, -1);
        }

        public bool IsAbsent() {
            return _absent;
        }

        public void SetAbsent() {
            this._absent = false;
        }

        public bool IsNotHunted() {
            return _notHunted;
        }

        public void SetNotHunted() {
            this._notHunted = true;
        }

        private class CircularSearchCreator
        {
            private bool _active = false;
            private int _size;
            private float _center;

            public int[] Pattern(float maxRadius, int requestedSize)
            {
                if (_active)
                {
                    throw new Exception();
                }

                _active = true;
                _size = requestedSize;
                _center = (_size - 1f) / 2f;
                //var result = new List<int>();
                int[] result = new int[_size * _size];
                bool[] found = new bool[_size * _size];
                int nextResult = 0;
                int smallerHalfSize = _size / 2;
                int largerHalfSize = (_size + 1) / 2;
                for (float radius = 0; radius < maxRadius; radius = radius + 0.01f)
                {
                    // kind of a pain to go in a circle
                    // so do a simple search in each quadrant
                    // as long as the steps are really small everything will end up in order

                    // upper right
                    for (int y = 0; y < largerHalfSize; y++)
                    {
                        for (int x = smallerHalfSize; x < _size; x++)
                        {
                            int index = x * _size + y;
                            if (found[index])
                            {
                                continue; // skip to next block; this is already in the patter
                            }

                            float distance = DistanceFromCenter(x, y);
                            if (distance > radius)
                            {
                                continue; // still too far; skip
                            }

                            //place in patter
                            result[nextResult++] = index;
                            found[index] = true;
                        }
                    }

                    //lower right
                    for (int x = _size - 1; x >= smallerHalfSize; x--)
                    {
                        // out to in
                        for (int y = largerHalfSize; y < _size; y++)
                        {
                            int index = x * _size + y;
                            if (found[index])
                            {
                                continue; // skip to next block; this is already in the patter
                            }

                            float distance = DistanceFromCenter(x, y);
                            if (distance > radius)
                            {
                                continue; // still too far; skip
                            }

                            //place in patter
                            result[nextResult++] = index;
                            found[index] = true;
                        }
                    }

                    //lower left
                    for (int y = _size - 1; y >= largerHalfSize - 1; y--)
                    {
                        // out to in
                        for (int x = smallerHalfSize - 1; x > -1; x--)
                        {
                            int index = x * _size + y;
                            if (found[index])
                            {
                                continue; // skip to next block; this is already in the patter
                            }

                            float distance = DistanceFromCenter(x, y);
                            if (distance > radius)
                            {
                                continue; // still too far; skip
                            }

                            //place in pattern
                            result[nextResult++] = index;
                            found[index] = true;
                        }
                    }

                    //upper left
                    for (int x = 0; x < smallerHalfSize; x++)
                    {
                        // out to in
                        for (int y = largerHalfSize - 1; y > -1; y--)
                        {
                            int index = x * _size + y;
                            if (found[index])
                            {
                                continue; // skip to next block; this is already in the patter
                            }

                            float distance = DistanceFromCenter(x, y);
                            if (distance > radius)
                            {
                                continue; // still too far; skip
                            }

                            //place in pattern
                            result[nextResult++] = index;
                            found[index] = true;
                        }
                    }
                }

                _active = false;
                if (nextResult < result.Length)
                {
                    int[] newResult = new int[nextResult];
                    System.Array.Copy(result, 0, newResult, 0, newResult.Length);
                    result = newResult;
                }

                return result;
            }

            private float DistanceFromCenter(int x, int y)
            {
                return (float) Math.Sqrt(Math.Pow((x - this._center), 2) + Math.Pow((y - this._center), 2));
            }
        }
     }
}