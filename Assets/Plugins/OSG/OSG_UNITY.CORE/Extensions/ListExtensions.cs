using System.Collections;
using System.Collections.Generic;

namespace OSG
{
    public static class ListExtensions
    {
        /// <summary>
        /// Replaces duplicated elements with nulls.
        /// </summary>
        public static IList EmptyDuplicates<T>(this IList list)
        {
            List<T> tempList = new List<T>();

            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                T element = (T)list[i];

                if (element == null)
                    continue;

                if (tempList.Contains(element))
                    list[i] = null;
                else
                    tempList.Add(element);
            }

            return list;
        }

        /// <summary>
        /// Removes all duplicates from a list.
        /// </summary>
        public static IList RemoveDuplicates<T>(this IList list)
        {
            List<T> tempList = new List<T>();

            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                T element = (T)list[i];

                if (element == null)
                    continue;

                if (tempList.Contains(element))
                {
                    list.RemoveAt(i);
                    i++;
                }
                else
                    tempList.Add(element);
            }

            return list;
        }
    }
}