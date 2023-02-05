using System;
using System.Collections;
using System.Reflection;
using OSG.Core.EventSystem;
using OSG.Services;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OSG.EventSystem
{
    /// <summary>
    /// Used by EventEmitter to 
    /// store event sources
    /// </summary>
    [Serializable]
    public class EventSource
    {
        [SerializeField] public string eventName;
        private IEmitable evt;

        [SerializeField] private int Int32Parameter;
        [SerializeField] private string StringParameter;
        [SerializeField] private float FloatParameter;
        [SerializeField] private bool BooleanParameter;
        [SerializeField] private Object ObjectParameter;

        private object[] parameters;
        private MethodInfo eventInvokeMethodInfo;
        private ServiceRef<IEventSystem> eventSystem = new ServiceRef<IEventSystem>();

        public void Init()
        {
            FieldInfo fieldInfo = EventContainerHelper.GetEventFieldInfo(eventName);

            if (fieldInfo == null)
            {
                Debug.LogError("No event named " + eventName + " found");
                return;
            }

            var eventContainer = eventSystem.Value.Get(fieldInfo.DeclaringType);
            if (eventContainer == null)
            {
                Debug.LogError("No Event container found");
                return;
            }
            
            var e = fieldInfo.GetValue(eventContainer);
            if (e == null)
            {
                Debug.LogError("Event " + eventName + " is null");
                return;
            }

            evt = fieldInfo.GetValue(eventContainer) as IEmitable;
            if (evt == null)
            {
                Debug.LogError("Event " + eventName + " is not an IEmitable");
                return;
            }

            
            ParameterInfo[] parameterInfos = EventContainerHelper.GetEventParameters(fieldInfo.FieldType, out eventInvokeMethodInfo);
            
            parameters = new object[parameterInfos.Length];
            for (int index = 0; index < parameterInfos.Length; index++)
            {
                parameters[index] = GetParameterValue(parameterInfos[index]);
            }
        }

        private object GetParameterValue(ParameterInfo parameterInfo)
        {
            Type pType = parameterInfo.ParameterType;
            if (pType == typeof(int))
            {
                return Int32Parameter;
            }
            if (pType == typeof(string))
            {
                return StringParameter;
            }
            if (pType == typeof(float))
            {
                return FloatParameter;
            }
            if (pType == typeof(bool))
            {
                return BooleanParameter;
            }
            if (pType.DerivesFrom(typeof(Object)))
            {
                return ObjectParameter;
            }

            return null;
        }

        public void Emit()
        {
            eventInvokeMethodInfo?.Invoke(evt,parameters);
        }
    }

    /// <summary>
    /// Allows to emit a user defined set of events
    /// </summary>
    public class EventEmitter : OSGMono
    {
        [SerializeField] private EventSource[] eventsSource;

        public override IEnumerator Init()
        {
            for (int i = 0; i < eventsSource.Length; ++i)
            {
                eventsSource[i].Init();
            }
            yield break;
        }

        public void EmitAll()
        {
            for (int i = 0; i < eventsSource.Length; ++i)
            {
                eventsSource[i].Emit();
            }
        }

        /// <summary>
        /// Emit the event of a given name
        /// Meant to be called via Reflection, by a clic on Button, for example
        /// </summary>
        /// <param name="eventName"></param>
        public void Emit(string eventName)
        {
            for (int i = 0; i < eventsSource.Length; ++i)
            {
                if (eventsSource[i].eventName == eventName)
                {

                    eventsSource[i].Emit();
                    return;
                }
            }

            Debug.LogError("No event named <b>" + eventName + "</b> exists");
        }
    }
}