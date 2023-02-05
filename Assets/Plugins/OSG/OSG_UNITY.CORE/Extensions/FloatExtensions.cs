using UnityEngine;

namespace OSG
{
    public static class FloatExtensions
    {
        /// <summary>
        /// Rounds a float value to a specified step.
        /// </summary>
        public static float RoundValue(this float value, float step)
        {
            return Mathf.Round(value / step) * step;
        }

        /// <summary>
        /// Clamps the float value to the given minimum one.
        /// </summary>
        public static float Min(this float value, float min)
        {
            if (value < min)
                value = min;

            return value;
        }

        /// <summary>
        /// Clamps the float value to the given maximum one.
        /// </summary>
        public static float Max(this float value, float max)
        {
            if (value > max)
                value = max;

            return value;
        }
    }
}