using UnityEngine;

namespace FigmentGames
{
    public class MinValue : PropertyAttribute
    {
        public readonly float minValue;

        public MinValue(float minValue)
        {
            this.minValue = minValue;
        }

        public MinValue(int minValue)
        {
            this.minValue = minValue;
        }
    }

    public class MaxValue : PropertyAttribute
    {
        public readonly float maxValue;

        public MaxValue(float maxValue)
        {
            this.maxValue = maxValue;
        }

        public MaxValue(int maxValue)
        {
            this.maxValue = maxValue;
        }
    }
}
