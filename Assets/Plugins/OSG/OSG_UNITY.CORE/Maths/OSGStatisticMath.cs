using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OSG
{
    public static partial class OSGMath
    {
        /// <summary>
        /// Return median of a collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static float Median(this IEnumerable<int> source) 
        {
            int[] data = source.OrderBy(n => n).ToArray();

            if (data.Length % 2 == 0)
                return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2f;
            else
                return data[data.Length / 2];
        }

        /// <summary>
        /// Return median of a collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static float Median(this IEnumerable<float> source)
        {
            float[] data = source.OrderBy(n => n).ToArray();

            if (data.Length % 2 == 0)
                return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2f;
            else
                return data[data.Length / 2];
        }


        /// <summary>
        /// Return median of a collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static double Median(this IEnumerable<double> source)
        {
            double[] data = source.OrderBy(n => n).ToArray();

            if (data.Length % 2 == 0)
                return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2d;
            else
                return data[data.Length / 2];
        }

        /// <summary>
        /// Return median of a collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static float Median(this IEnumerable<char> source)
        {
            char[] data = source.OrderBy(n => n).ToArray();

            if (data.Length % 2 == 0)
                return ((data[data.Length / 2 - 1] + data[data.Length / 2]) / 2);
            else
                return data[data.Length / 2];
        }

        /// <summary>
        /// return median of a collection using selector
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static float Median<T>(this IEnumerable<T> source, Func<T, int> selector)
        {
            return source.Select(selector).Median();
        }

        /// <summary>
        /// return median of a collection using selector
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static float Median<T>(this IEnumerable<T> source, Func<T, float> selector)
        {
            return source.Select(selector).Median();
        }

        /// <summary>
        /// return median of a collection using selector
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static double Median<T>(this IEnumerable<T> source, Func<T, double> selector)
        {
            return source.Select(selector).Median();
        }


        /// <summary>
        /// return median of a collection using selector
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static float Median<T>(this IEnumerable<T> source, Func<T, char> selector)
        {
            return source.Select(selector).Median();
        }
    }
}
