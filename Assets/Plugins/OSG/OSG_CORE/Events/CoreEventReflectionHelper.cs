using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OSG.Core.EventSystem
{
    /// <summary>
    /// Caches the various MethodInfo needed to 
    /// add or remove a listener by reflection to a core event
    /// </summary>
    public class CoreEventReflectionHelper
    {
        public readonly CoreEventBase destinationEvent;
        private MethodInfo addMethod;
        const BindingFlags methodBindingFlags = BindingFlags.Instance 
                                              | BindingFlags.NonPublic 
                                              | BindingFlags.Public 
                                              | BindingFlags.FlattenHierarchy;

        private MethodInfo GetMethod(Func<MethodInfo, bool> check)
        {
            MethodInfo[] infos = destinationEvent.GetType().GetMethods(methodBindingFlags);
            return infos.FirstOrDefault(check);
        }

        // Get the event's AddListener's MethodInfo
        private MethodInfo AddMethod => addMethod 
                                        ?? (addMethod = GetMethod(m => m.Name == "AddListener" && m.GetParameters().Length==1));


        public CoreEventReflectionHelper(string eventName, IEnumerable<CoreEventContainer> containers)
        {
            foreach (CoreEventContainer eventContainer in containers)
            {
                FieldInfo fieldInfo = eventContainer.GetType().GetField(eventName);
                if (fieldInfo != null)
                {
                    destinationEvent = fieldInfo.GetValue(eventContainer) as CoreEventBase;
                    break;
                }
            }

            if (destinationEvent == null)
            {
                throw new Exception(eventName + " not found");
            }
        }

        public CoreEventReflectionHelper(string eventName, 
            object target, 
            MethodInfo targetMethod, 
            IEnumerable<CoreEventContainer> containers) : this(eventName, containers)
        
        {
            AddListener(target, targetMethod);
        }

        private void AddListener(object target, MethodInfo targetMethod)
        {
            // Get the type of the event.AddListener's Parameter 
            Type parameterType = AddMethod.GetParameters()[0].ParameterType;
            // Create a delegate out of this type, to the target's method
            var targetDelegate = Delegate.CreateDelegate(parameterType, target, targetMethod);
            // Register the target
            AddMethod.Invoke(destinationEvent, new object[] {targetDelegate});
        }
    }
}