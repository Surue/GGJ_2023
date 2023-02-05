using UnityEngine;

namespace OSG
{
    public class MinValue : PropertyAttribute
    {
        public readonly float minFloat;
        public readonly int minInt;

        public MinValue(float minFloat)
        {
            this.minFloat = minFloat;
            this.minInt = (int)minFloat;
        }
    }

    public class MaxValue : PropertyAttribute
    {
        public readonly float maxFloat;
        public readonly int maxInt;

        public MaxValue(float maxFloat)
        {
            this.maxFloat = maxFloat;
            this.maxInt = (int)maxFloat;
        }
    }
}