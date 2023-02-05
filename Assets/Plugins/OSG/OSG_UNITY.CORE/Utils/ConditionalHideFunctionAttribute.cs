// Old Skull Games
// Bernard Barthelemy
// Tuesday, October 10, 2017


using System;
using System.Reflection;
using UnityEngine;

namespace OSG
{
    /// <summary>
    /// Allows to hide some fields in the inspector, based on the return value of a
    /// given method
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ConditionalHideFunctionAttribute : PropertyAttribute
    {
        private readonly string ConditionalMethod;
        public readonly bool HideInInspector;

        public ConditionalHideFunctionAttribute(string methodName, bool hide = false)
        {
            HideInInspector=hide;
            ConditionalMethod = methodName;
        }

        public delegate  object InstanceProviderDelegate();
        public bool GetConditionalHideAttributeResult(Type declaringType, InstanceProviderDelegate instanceProvider = null)
        {
            if(string.IsNullOrEmpty(ConditionalMethod))
                return true;
            if(instanceProvider == null)
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
                if(method==null)
                    return true;
                // static method may not like to have an owner
                instanceProvider = () => null;
            }
            else
            {
                if(instanceProvider() == null)
                    return true;
            }
            object instance = instanceProvider();
            bool enabled;
            try{
                // when put on an array, the attribute is on all element on the array as well, 
                // leading to different types
                if(instance != null && !instance.GetType().DerivesFrom(method.DeclaringType))
                    return true;

                enabled = (bool) method.Invoke(instance, new object[] { });
            }
            catch(Exception e)
            {
                UnityEngine.Debug.Log("Paye ton foutage de gueule " +instance+" "+ method.Name);
                UnityEngine.Debug.LogException(e);
                enabled = true;
            }
            return enabled;
        }

        private MethodInfo GetStaticMethod(Type declaringType)
        {
            return declaringType.GetMethod(ConditionalMethod,
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        }

        private MethodInfo GetInstanceMethod(Type declaringType)
        {
            return declaringType.GetMethod(ConditionalMethod,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

    }
}