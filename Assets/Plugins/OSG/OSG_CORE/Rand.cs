// Old Skull Games
// Pierre Planeau
// Thursday, June 14, 2018

using System;

namespace OSG.Core
{
    /// <summary>
    /// Alternative to System.Random to produce pseudo random numbers based on a seed.
    /// </summary>
    public class Rand
    {
        protected UInt32 seed;  // 100% random seed value
        protected Int32 minVal;
        protected Int32 maxVal;
        protected Int64 factor;


        /// <summary>
        /// Constructs a pseudo random number generator based on the given seed.
        /// All numbers produced by the function Next() will be within the range [minVal, maxVal].
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="minVal"></param>
        /// <param name="maxVal"></param>
        public Rand(UInt32 seed, Int32 minVal, Int32 maxVal)
        {
            Initialize(seed, minVal, maxVal);
        }

        /// <summary>
        /// Constructs a pseudo random number generator based on the given seed (converted to unsigned int).
        /// All numbers produced by the function Next() will be within the range [minVal, maxVal].
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="minVal"></param>
        /// <param name="maxVal"></param>
        public Rand(Int32 seed, Int32 minVal, Int32 maxVal)
        {
            Initialize((UInt32) seed, minVal, maxVal);
        }

        protected void Initialize(UInt32 seed, Int32 minVal, Int32 maxVal)
        {
            this.seed = seed;
            this.minVal = minVal;
            this.maxVal = maxVal;

            factor = CalculateFactor(minVal, maxVal);
        }

        protected Int64 CalculateFactor(Int32 min, Int32 max)
        {
            Int32 range = (max - min) + 1;

            if (range == 0)
                return (Int64)UInt32.MaxValue + 1;

            return UInt32.MaxValue / range;
        }

        private UInt32 InternalNext()
        {
            seed ^= seed << 13;
            seed ^= seed >> 17;
            seed ^= seed << 5;
            return seed;
        }

        /// <summary>
        /// Returns a random number within the interval given when the object was constructed.
        /// </summary>
        /// <returns></returns>
        public Int32 Next()
        {
            return (Int32)(InternalNext() / factor) + minVal;
        }

        /// <summary>
        /// Returns a random number within the [min, max] interval.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public Int32 Next(Int32 min, Int32 max)
        {
            return (Int32)(InternalNext() / CalculateFactor(min, max)) + min;
        }
    }
}
