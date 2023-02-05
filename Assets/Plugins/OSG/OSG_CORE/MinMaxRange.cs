/* MinMaxRangeAttribute.cs
* by Eddie Cameron – For the public domain
* —————————-
* Use a MinMaxRange class to replace twin float range values (eg: float minSpeed, maxSpeed; becomes MinMaxRange speed)
* Apply a [MinMaxRange( minLimit, maxLimit )] attribute to a MinMaxRange instance to control the limits and to show a
* slider in the inspector
*/
using System;
namespace OSG.Core
{
    public abstract class MinMaxRange<T> : ICloneable
    {
        //protected static Random random = new Random();

        public T rangeStart, rangeEnd; // cannot be here because it breaks the unity network serializer TODO: make it an abstract property if needed in a generic 
#if ENABLE_NEWTONSOFT
        [Newtonsoft.Json.JsonIgnore]
#endif
        public abstract T Value { get; }

        public abstract object Clone();

        /// <summary>
        /// Returns a random value between [rangeStart, rangeEnd] using the provided random function.
        /// </summary>
        /// <param name="MinMaxInclusiveRandomFunction">Inclusive random function that takes a minimum and a maximum value as parameters.</param>
        /// <returns></returns>
        public T Roll(System.Func<T, T, T> MinMaxInclusiveRandomFunction)
        {
            return MinMaxInclusiveRandomFunction(rangeStart, rangeEnd);
        }

        public abstract bool IsInRange(T value);
#if ENABLE_NEWTONSOFT
        [Newtonsoft.Json.JsonIgnore]
#endif
        public abstract T rangeMiddle { get; }


        public override string ToString()
        {
            return $"MinMaxRange({rangeStart}, {rangeEnd})";
        }
    }

    [System.Serializable]
    public class MinMaxRange : MinMaxRange<float>
    {
        //public float rangeStart, rangeEnd;

        public override float Value
        {
            get
            {
                return Roll((min, max) =>
                {
                    return CoreMath.Lerp(min, max, (float)new Random().NextDouble());
                });
            }
            //get { return (float)CoreMath.Lerp(rangeStart, rangeEnd, (float)random.NextDouble()); }
        }

        public override object Clone()
        {
            MinMaxRange c = new MinMaxRange();
            c.rangeStart = this.rangeStart;
            c.rangeEnd = this.rangeEnd;
            return c;
        }

        public override bool IsInRange(float value)
        {
            return !((value > rangeEnd) || (value < rangeStart));
        }

        public override float rangeMiddle
        {
            get { return rangeStart + ((rangeEnd - rangeStart) * 0.5f); }
        }
    }

    [System.Serializable]
    public class MinMaxRangeInt : MinMaxRange<int>
    {
        //public int rangeStart, rangeEnd;


        public override int Value
        {
            get
            {
                return Roll((min, max) => { return new Random().Next(min, max + 1); });
            }
            //get { return random.Next(rangeStart, rangeEnd); }
        }

        public override object Clone()
        {
            MinMaxRangeInt c = new MinMaxRangeInt
            {
                rangeStart = this.rangeStart,
                rangeEnd = this.rangeEnd
            };
            return c;
        }

        public override bool IsInRange(int value)
        {
            return !((value > rangeEnd) || (value < rangeStart));
        }

        public override int rangeMiddle
        {
            get { return rangeStart + ((rangeEnd - rangeStart) / 2); }
        }
    }
}