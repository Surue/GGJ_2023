using System;
using System.Collections.Generic;

namespace OSG.Core
{
    public static class CollectionsExtensions
    {
        private static readonly Random random = new Random();

        public static T Modulo<T>(this T[] array, int index)
        {
#if DEBUG
            if (array.Length == 0)
                throw new Exception("Array is empty");
#endif
            return array[index % array.Length];
        }

        public static T GetRandomElementCopy<T>(this T[] array) where T : struct
        {
#if DEBUG
            if (array.Length == 0)
                throw new Exception("Array is empty");
#endif
            return array[random.Next(0, array.Length)];
        }


        public static T GetElementAtRandomIndex<T>(this T[] array) where T : class
        {
#if DEBUG
            if(array.Length == 0)
                throw new Exception("Array is empty");
#endif
            return array[random.Next(0, array.Length)];
        }
        

        public static T RemoveRandomElement<T>(this List<T> array)
        {
#if DEBUG
            if (array.Count == 0)
                throw new Exception("Array is empty");
#endif
            int index = random.Next(0, array.Count);
            T e = array[index];
            array.RemoveAt(index);
            return e;
        }

        public static S GetRandomElementCopy<S>(this List<S> list) where S : struct
        {
#if DEBUG
            if (list.Count == 0)
                throw new Exception("Array is empty");
#endif
            return list[random.Next(0, list.Count)];
        }


        public static T GetElementAtRandomIndex<T>(this List<T> list) where T : class
        {
#if DEBUG
            if(list.Count == 0)
                throw new Exception("Array is empty");
#endif
            return list[random.Next(0, list.Count)];
        }

        public static bool InRange<T>(this T[] array, int index)
        {
            return array != null && (uint) index < array.Length;
        }

        public static bool InRange<T>(this List<T> list, int index)
        {
            return list != null && (uint) index < list.Count;
        }

        public static T[] GetCopy<T>(this T[] array)
        {
            if (array == null) return null;
            T[] newArray = new T[array.Length];
            for (int index = array.Length; --index >= 0;)
            {
                newArray[index] = array[index];
            }
            return newArray;
        }

        /// <summary>
        /// Transforms an array into a List.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this T[] array)
        {
            List<T> list = new List<T>(array.Length);
            list.AddRange(array);
            return list;
        }


        /// <summary>
        /// Swap 2 elements of the list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="indexA"></param>
        /// <param name="indexB"></param>
        /// <returns>The lsit itself for chaining method</returns>
        public static IList<T> Swap<T>(this IList<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
            return list;
        }


        /// <summary>
        /// Swap 2 elements of the list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="indexA"></param>
        /// <param name="indexB"></param>
        /// <returns>The lsit itself for chaining method</returns>
        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            Random rng = new Random();
            for (int i = 0; i < list.Count; i++)
            {
                list.Swap(i, rng.Next(i, list.Count));
            }

            return list;
        }
    }
}