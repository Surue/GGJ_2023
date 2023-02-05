using System;
using OSG.Core.EventSystem;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OSG.EventSystem
{
    /// <summary>
    /// Used by EventReceiver to store event targets (receivers)
    /// </summary>
    [Serializable]
    public class EventTarget
    {
        [SerializeField] private string eventName;
        [SerializeField] private Object target;
        [SerializeField] private SerializableMethodInfo method;

        private  readonly EventSystemRef<CoreEventContainer, CoreEventSystem> eventSystem = new EventSystemRef<CoreEventContainer, CoreEventSystem>();

        public override string ToString()
        {
            return $"Event Target:\n<b>eventName</b> {eventName}\n<b>target</b> {(target == null ? "null" : target.name)}\n<b>method</b> {method}";
        }

        public bool Register()
        {
            using (eventSystem.SetRegisteringContext(this))
            {
                if(!eventSystem.Value.Register(eventName, target, method.Info, Debug.LogError))
                {
                    Debug.LogError($"Couldn't Register to event {eventName} for target {target.name} with method {method.Info.Name}");
                    return false;
                }
            }

            return true;
        }

        public void Unregister()
        {
            eventSystem.UnregisterFromAllEvents(this);
        }
    }
}