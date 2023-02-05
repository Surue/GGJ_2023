using System;
using System.Reflection;
using System.Text;
//using UnityEngine;

namespace OSG
{
    public static class SystemExtensions
    {
        /// <summary>
        /// Allows to check if a type extends another base type.<br/>
        /// 
        /// **WORKS WITH TEMPLATED TYPES**
        /// </summary>
        /// <param name="extendType"></param>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public static bool DerivesFrom(this Type extendType, Type baseType)
        {
            while (!baseType.IsAssignableFrom(extendType))
            {
                if (extendType == typeof(object) || extendType == null)
                {
                    return false;
                }
                if (extendType.IsGenericType && !extendType.IsGenericTypeDefinition)
                {
                    extendType = extendType.GetGenericTypeDefinition();
                }
                else
                {
                    extendType = extendType.BaseType;
                }
            }
            return true;
        }

        /// <summary>
        /// simple way to get the attribute of a given attributetype on a general type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static T GetCustomAttribute<T> (this Type type, bool inherit=false) where T : Attribute
        {
            var attributes = type.GetCustomAttributes(typeof(T), inherit);
            if(attributes.Length == 0)
                return null;
            if(attributes.Length>1)
                throw new Exception("Don't use this function on a type that has multiple "+typeof(T).Name+" attributes");
            return attributes[0] as T;
        }


        public static T ReflectionInvoke<T>(this object obj, string methodName, params object[] callParameters)
        {
            Type type = obj.GetType();
            MethodInfo info = type.GetMethod(methodName, BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy);
            return info == null ? default(T) : (T) info.Invoke(obj, callParameters);
        }

        private static StringBuilder sb;
        public static string LongToMemory(this long l)
        {
            sb = sb ?? new StringBuilder();
            sb.Clear();
            bool negative = l < 0;
            if (negative)
            {
                l = -l;
                sb.Append("-");
            }

            const long gigChunk = 0x40000000L;
            long gig = (l / gigChunk);
            bool write = gig > 0;
            if (write)
            {
                sb.Append($"{gig}G ");
            }

            l -= gig * gigChunk;
            const long megChunk = 0x100000L;
            long meg = (l / megChunk);
            write |= meg > 0;
            if (write)
            {
                sb.Append($"{meg}M ");
            }

            l -= meg * megChunk;
            const long kilChunk = 0x400L;
            long kil = l / kilChunk;
            write |= kil > 0;
            if (write)
            {
                sb.Append($"{kil}K ");
            }

            l -= kil * kilChunk;
            sb.Append($"{l}B");
            return sb.ToString();
        }
    }
}