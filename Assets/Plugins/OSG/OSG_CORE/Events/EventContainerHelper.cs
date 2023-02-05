using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OSG.Services;

namespace OSG.Core.EventSystem
{
    public static class EventContainerHelper
    {
        private static ServiceRef<IEventSystem> system = new ServiceRef<IEventSystem>();
        private static Dictionary<Type, List<FieldInfo>> containersFieldInfos = new Dictionary<Type, List<FieldInfo>>();
        private static Dictionary<Type, IEnumerable<FieldInfo>> _eventsOfType;


        


        public const BindingFlags MethodBindingFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;

        public static FieldInfo GetEventFieldInfo(string eventName)
        {
            return GetAllEventInfos().FirstOrDefault(info => info.Name == eventName);
        }

        public static ParameterInfo[] GetEventParameters(string eventName, out MethodInfo eventInvokeMethodInfo)
        {
            FieldInfo info = GetEventFieldInfo(eventName);
            eventInvokeMethodInfo = null;
            return info == null ? new ParameterInfo[0] : GetEventParameters(info.FieldType, out eventInvokeMethodInfo);
        }

        public static ParameterInfo[] GetEventParameters(Type eventType, out MethodInfo eventInvokeMethodInfo)
        {
            eventInvokeMethodInfo =
                eventType.GetMethod(nameof(CoreEvent.Invoke), MethodBindingFlags | BindingFlags.NonPublic);
            if (eventInvokeMethodInfo != null)
            {
                return eventInvokeMethodInfo.GetParameters();
            }
            else
            {
                string msg = ("No Emit function found on " + eventType.FullName);

                msg += "\nAvailable methods :";
                var methods = Enumerable.ToList(eventType.GetMethods(MethodBindingFlags | BindingFlags.NonPublic));
                methods.Sort((i0, i1) => string.CompareOrdinal(i0.Name, i1.Name));
                foreach (MethodInfo info in methods)
                {
                    msg += "\n" + info.Name + " in " + info.DeclaringType;
                }

                throw new Exception(msg);
            }
        }

        public static bool IsCompatible(MethodInfo targetMethodInfo, ParameterInfo[] eventInvokeParameters)
        {
            ParameterInfo[] targetParamInfos = targetMethodInfo.GetParameters();
            if (targetMethodInfo.ReturnType != typeof(void)) return false;
            if (targetParamInfos.Length != eventInvokeParameters.Length)
                return false;

            if (targetParamInfos.Length == 0) return true;

            if (targetParamInfos.Length > 1) return false;



            for (int i = 0; i < targetParamInfos.Length; ++i)
            {
                Type targetType = targetParamInfos[i].ParameterType;
                Type eventType = eventInvokeParameters[i].ParameterType;

                if (!eventType.DerivesFrom(targetType))
                {
                    return false;
                }
            }

            return true;
        }
        

        public static IEnumerable<FieldInfo> GetEventsWithPayloadType(Type t)
        {
            _eventsOfType = _eventsOfType ?? new Dictionary<Type, IEnumerable<FieldInfo>>();
            if (!_eventsOfType.TryGetValue(t, out var l))
            {
                l = FindEventsOfType(t);
                _eventsOfType.Add(t, l);
            }

            return l;
        }

        private static IEnumerable<FieldInfo> FindEventsOfType(Type payloadType)
        {
            List<FieldInfo> l = new List<FieldInfo>();
            foreach (FieldInfo info in GetAllEventInfos())
            {
                Type eventType = GetBaseEventType(info.FieldType);
                if (eventType == null)
                    continue;

                if (eventType.IsGenericType)
                {
                    Type[] args = eventType.GetGenericArguments();
                    if (args.Length > 0 && args[0].DerivesFrom(payloadType))
                        l.Add(info);
                }
                else
                {
                    if (typeof(void) == payloadType)
                    {
                        l.Add(info);
                    }
                }
            }
            return l;
        }

        private static Type GetBaseEventType(Type type)
        {
            return type == null || type == typeof(CoreEvent) || type == typeof(CoreEvent<>)
                ? type
                : type.BaseType;
        }

        public static Type GetEventPayloadType(string eventName)
        {
            foreach (var eventInfos in containersFieldInfos)
            {
                foreach (FieldInfo eventInfo in eventInfos.Value)
                {
                    if (eventInfo.Name == eventName)
                    {
                        return GetEventPayloadType(eventInfo);
                    }
                }
            }
            return null;
        }

        public static Type GetEventPayloadType(FieldInfo eventInfo)
        {
            Type baseType = GetBaseEventType(eventInfo.FieldType);
            if (baseType.IsGenericType)
            {
                Type[] args = baseType.GetGenericArguments();
                return args.Length > 0 ? args[0] : null;
            }

            return typeof(void);
        }

        public static void SetSystemForAllEventsInContainer(CoreEventSystem system, CoreEventContainer container)
        {
            var containerType = container.GetType();
            List<FieldInfo> eventInfos = GetEventInfos(containerType);

            if(containersFieldInfos.TryGetValue(containerType, out var _dummy))
            {
                CoreDebug.LogWarning($"Duplicated container type {containerType.Name}");
            }
            else
            {
                containersFieldInfos.Add(containerType, eventInfos);
            }

            foreach (FieldInfo fieldInfo in eventInfos)
            {
                CoreEventBase eventObject = fieldInfo.GetValue(container) as CoreEventBase;
                eventObject?.SetEventSystem(system);
            }
        }

        public static List<FieldInfo> GetAllEventInfos()
        {
            List<FieldInfo> eI = new List<FieldInfo>(50);
            AssemblyScanner.Register(type =>
            {
                if (type.DerivesFrom(typeof(CoreEventContainer)))
                {
                    ScanTypeForEvents(type, eI);
                }
            });
            AssemblyScanner.Scan(() =>
            {
                if (eI.Any())
                {
                    eI.Sort((i1, i2) => string.CompareOrdinal(i1.Name, i2.Name));
                    CheckForDuplicatedNameInSortedList(eI);
                }
            });
            return eI;
        }

        private static void CheckForDuplicatedNameInSortedList(List<FieldInfo> eI)
        {
            FieldInfo lastInfo = eI[0];
            for (var index = 1; index < eI.Count; index++)
            {
                FieldInfo info = eI[index];
                int compareOrdinal = string.CompareOrdinal(info.Name, lastInfo.Name);
                if(compareOrdinal < 0)
                {
                    throw new Exception("The list isn't sorted");
                }

                if (compareOrdinal == 0)
                {
                    throw new Exception(
                        $"Event {info.Name} is in at least two different event containers : {info.DeclaringType.Name} and {lastInfo.DeclaringType.Name}");
                }

                lastInfo = info;
            }
        }

        //public static List<FieldInfo> EventInfos => _eventsInfos;

        public static List<FieldInfo> GetEventInfos(Type containerType)
        {
            if (!containersFieldInfos.TryGetValue(containerType, out var eI))
            {
                eI = new List<FieldInfo>();
                ScanTypeForEvents(containerType, eI);
            }
            return eI;
        }


        private static void ScanTypeForEvents(Type eventContainerType, List<FieldInfo> addThere)
        {
            var allMembers = eventContainerType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < allMembers.Length; i++)
            {
                FieldInfo info = allMembers[i];
                bool isAnEvent = (info.FieldType.DerivesFrom(typeof(CoreEvent)) || info.FieldType.DerivesFrom(typeof(CoreEvent<>)));
                if (isAnEvent)
                {
                    addThere.Add(info);
                }
            }
        }

    }


}