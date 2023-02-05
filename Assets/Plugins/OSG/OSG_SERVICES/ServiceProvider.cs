// Old Skull Games
// Benoit Constantin
// Wednesday, March 20, 2019

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//https://digitalrune.github.io/DigitalRune-Documentation/html/619b1341-c6a1-4c59-b33d-cc1f799402dc.htm
namespace OSG.Services
{
    public static class ServiceProvider
    {

        private static Dictionary<Type, List<object>> serviceDictionary = new Dictionary<Type, List<object>>();

        /// <summary>
        /// Get the first service of the type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetService<T>() where T : class
        {
            Type interfaceType = typeof(T);
            serviceDictionary.TryGetValue(interfaceType, out var service);
            return service?.Count > 0 ? (T) service[0] : null;
        }

        /// <summary>
        /// Get the first service of the type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetService<T>(System.Predicate<T> match) where T : class
        {
            serviceDictionary.TryGetValue(typeof(T), out var services);
            if (services != null && services.Count > 0)
            {
                List<T> convertedServices = services.Cast<T>().ToList();
                return convertedServices.Find(match);
            }

            return null;
        }

        /// <summary>
        /// Get the all services of the type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> GetServices<T>() where T : class
        {
            List<object> services;
            serviceDictionary.TryGetValue(typeof(T), out services);

            if (services != null)
            {
                List<T> convertedServices = new List<T>(services.Count);

                for (int i = 0; i < services.Count; i++)
                {
                    convertedServices.Add((T) services[i]);
                }

                return convertedServices;
            }
            else
                return null;
        }


        public static void AddService(Type @interface, object service)
        {
            if(service==null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            serviceDictionary.TryGetValue(@interface, out var services);
            if (services == null)
            {
                services = new List<object>();
                serviceDictionary.Add(@interface, services);
            }
            services.Add(service);
        }

        public static void AddService<T>(T service)
        {
            AddService(typeof(T), service);
        }


        public static void RemoveService<T>(T service)
        {
            List<object> services;

            serviceDictionary.TryGetValue(service.GetType(), out services);
            if (services != null)
            {
                services.Remove(service);
            }
        }


        public static StringBuilder Report(StringBuilder sb)
        {
            sb.AppendLine("Current Services");
            foreach (var kvp in serviceDictionary)
            {
                sb.Append($"  {kvp.Key.Name} => ");
                foreach (var o in kvp.Value)
                {
                    sb.Append($" {o.ToString()}");
                }

                sb.AppendLine();
            }

            return sb;
        }

        public static void Dispose()
        {

        }
    }
}