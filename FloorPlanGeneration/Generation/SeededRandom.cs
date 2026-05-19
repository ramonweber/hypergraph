using System;

namespace FloorPlanGeneration.Generation
{
    internal sealed class SeededRandom
    {
        private uint _state;

        public SeededRandom(int seed)
        {
            _state = seed == 0 ? 2463534242u : unchecked((uint)seed);
        }

        public double NextDouble()
        {
            _state = unchecked((_state * 1664525u) + 1013904223u);
            return _state / (double)uint.MaxValue;
        }

        public int Next(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                return minInclusive;
            }

            return minInclusive + (int)Math.Floor(NextDouble() * (maxExclusive - minInclusive));
        }

        public double Range(double min, double max)
        {
            return min + ((max - min) * NextDouble());
        }
    }
}
