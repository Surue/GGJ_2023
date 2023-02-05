
using UnityEngine;

namespace OSG
{
    public class MinMaxRangeAttribute : PropertyAttribute
    {
        public float minLimit, maxLimit;

        public MinMaxRangeAttribute(float minLimit, float maxLimit)
        {
            this.minLimit = minLimit;
            this.maxLimit = maxLimit;
        }
    }
}
