// Old Skull Games
// Pierre Planeau
// Wednesday, January 09, 2019


using System;
using System.Reflection;
using UnityEngine;

namespace OSG
{
    /// <summary>
    /// Displays the (string) information returned by the given function as a LabelField.
    /// </summary>
    //[AttributeUsage(AttributeTargets.Field)]
    public class DisplayInfosAttribute : PropertyAttribute
    {
        public const string redSign = "<b><color=red>/!\\</color></b>";
        public const string redSigns = redSign + redSign + redSign + "\n";
        public const string OKMessage = "OK";

        private readonly string InfosMethod;

        public DisplayInfosAttribute(string methodName)
        {
            InfosMethod = methodName;
        }

        public delegate object InstanceProviderDelegate();
        public string GetInfosToDisplay(Type declaringType, InstanceProviderDelegate instanceProvider = null)
        {
            if (string.IsNullOrEmpty(InfosMethod))
                return "";
            if (instanceProvider == null)
            {
                instanceProvider = () => null;
            }
            // let's check for Instance method first
            var method = GetInstanceMethod(declaringType);
            if (method == null)
            {
                // let's check for Instance method first
                // none found, we check for static
                method = GetStaticMethod(declaringType);
                // no method, do show 
                if (method == null)
                    return "";
                // static method may not like to have an owner
                instanceProvider = () => null;
            }
            else
            {
                if (instanceProvider() == null)
                    return "";
            }

            object instance = instanceProvider();
            string result;
            try
            {
                // when put on an array, the attribute is on all element on the array as well, 
                // leading to different types
                if (instance != null && !instance.GetType().DerivesFrom(method.DeclaringType))
                    return "";

                result = (string)method.Invoke(instance, new object[] { });
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("Paye ton foutage de gueule " + instance + " " + method.Name);
                UnityEngine.Debug.LogException(e);
                result = "";
            }
            return result;
        }

        private MethodInfo GetStaticMethod(Type declaringType)
        {
            return declaringType.GetMethod(InfosMethod,
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        }

        private MethodInfo GetInstanceMethod(Type declaringType)
        {
            return declaringType.GetMethod(InfosMethod,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

    }
}