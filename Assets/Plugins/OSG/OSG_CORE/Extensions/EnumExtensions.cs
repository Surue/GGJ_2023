// Old Skull Games
// Benoit Constantin
// Tuesday, September 10, 2019

using System;

namespace OSG.Core
{
    //Helper for working with enums
    public static class EnumExtensions
    {
#if UNITY_2019_1_OR_NEWER

        public interface IUnderlyingTypeProvider
        {
            Type GetUnderlyingType();
        }

        [System.Serializable]
        public abstract class EnumMask //cheat with Unity serialization
        {

        }


        [System.Serializable]
        public class EnumMask<T> : EnumMask, IUnderlyingTypeProvider where T : Enum
        {
            public int mask;

            public Type GetUnderlyingType()
            {
                return typeof(T);
            }

            /// <summary>
            /// Construct an EnumMask with all bits set to 1
            /// </summary>
            public EnumMask()
            {
                mask = SetAllBit<T>();
            }

            public EnumMask(T value)
            {
                mask = SetBitMask(0,value);
            }

            public bool IsSet(T value)
            {
                return IsSet<T>(mask, value);
            }

            /// <summary>
            /// Return if the mask is under the value (like a Low/High force, High is greater than Low)
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public bool IsBelowOrEquals(T value)
            {
                return IsBelowOrEquals<T>(mask, value);
            }
        }


        public static bool IsBelowOrEquals<T>(int mask, T value) where T : Enum
        {
            return mask <= (int)(object)value;
        }

        public static int SetBitMask<T>(int mask, T value) where T : Enum
        {
            return (mask | (int)(object)value);
        }

        public static int UnSetBitMask<T>(int mask, T value) where T : Enum
        {
            return (mask & ~(int)(object)value);
        }

        public static bool IsSet<T>(int mask, T value) where T : Enum
        {
            return (mask & (int)(object)value) != 0;
        }

        public static int SetAllBit<T>() where T : Enum
        {
            int mask = 0;

            foreach (T value in Enum.GetValues(typeof(T)))
            {
                mask = SetBitMask(mask, value);
            }

            return mask;
        }

        public static int SetAllBit(Type type)
        {
            int mask = 0;

            foreach (Enum value in Enum.GetValues(type))
            {
                mask = SetBitMask(mask, value);
            }

            return mask;
        }


        public static int SetBitMask(int mask, int index)
        {
            return (mask | (1<<index));
        }

        public static int UnSetBitMask(int mask, int index)
        {
            return (mask & ~ (1<<index));
        }

        public static bool IsSet(int mask, int index)
        {
            return (mask & (1<<index)) != 0;
        }
#endif
    }
}