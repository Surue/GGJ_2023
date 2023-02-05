// Old Skull Games
// Pierre Planeau
// Monday, May 21, 2018

using System;
using System.Collections.Generic;

namespace OSG.Core.EventSystem
{
    public abstract class CoreEventSystem : IEventSystem
    {
        private readonly Dictionary<Type, CoreEventContainer> eventContainers = new Dictionary<Type, CoreEventContainer>();

        public IEnumerable<CoreEventContainer> Containers => eventContainers.Values;


        public void AddEventContainer(CoreEventContainer container)
        {
            eventContainers[container.GetType()] = container;
            container.SetSystem(this);
        }

        protected CoreEventSystem(params CoreEventContainer[] containers)
        {
            listeners = new ListenersDictionnary();
            foreach (CoreEventContainer container in containers)
            {
                AddEventContainer(container);
            }
        }

        private class Subscription
        {
            public CoreEventBase @event;
            public Delegate @delegate;
            public Action Remove;
        }

        private class ListenersDictionnary : Dictionary<object, List<Subscription>>
        { }

        private ListenersDictionnary listeners;
        /// <summary>
        /// Used to specify the context object when registering events.
        /// </summary>
        private object registeringObject;


        public void Destroy()
        {
            foreach (var listener in listeners)
            {
                object context = listener.Key;
                if (context != null)
                {
                    List<Subscription> subscriptions;
                    if (!listeners.TryGetValue(context, out subscriptions))
                        continue;

                    for (int i = subscriptions.Count; --i >= 0;)
                    {
                        subscriptions[i].Remove();
                    }

                    subscriptions.Clear();
                }
            }

            listeners.Clear();
            listeners = null;
        }


        #region Register

#pragma warning disable 618 // shut up, I AM the CoreEventSystem
        /// <summary>
        /// You should not call this, only CoreEvent should be calling this.
        /// </summary>
        /// <param name="event"></param>
        /// <param name="action"></param>
        [Obsolete("This function is only meant to be called by the CoreEvent class. Use CoreEvent.AddListener instead.")]
        public void Register(CoreEvent @event, CoreAction action)
        {
            RegisterListener(@event, action, () => @event.DirectRemoveListener(action));
            @event.DirectAddListener(action);
        }

        /// <summary>
        /// You should not call this, only CoreEvent should be calling this.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="event"></param>
        /// <param name="action"></param>
        [Obsolete("This function is only meant to be called by the CoreEvent class. Use CoreEvent.AddListener instead.")]
        public void Register<T>(CoreEvent<T> @event, CoreAction<T> action)
        {
            RegisterListener(@event, action, () => @event.DirectRemoveListener(action));
            @event.DirectAddListener(action);
        }
#pragma warning restore 618

        /// <summary>
        /// Allow to register to an event by its name. Make sure you have a registering context before
        /// calling this.
        /// </summary>
        /// <param name="gameEventName">well, the event's name</param>
        /// <param name="target">the object that'll receive the event</param>
        /// <param name="call">the MethodInfo of the target's method to be called at event invocation</param>
        public bool Register(string gameEventName, object target, System.Reflection.MethodInfo call, Action<string> log)
        {
            try
            {
                var eventContainersValues = eventContainers.Values;
                CoreEventReflectionHelper desc = new CoreEventReflectionHelper(gameEventName, target, call, eventContainersValues);
                return true;
            }
            catch (Exception e)
            {
                log?.Invoke(e.Message);
            }

            return false;
        }


        protected void RegisterListener(CoreEventBase @event, Delegate @delegate, Action Remove)
        {
            List<Subscription> subscriptions;

#if THREAD_SAFE
            lock (registeringObject)
#endif
            {
                if (!listeners.TryGetValue(registeringObject, out subscriptions))
                {
                    subscriptions = new List<Subscription>();
                    listeners.Add(registeringObject, subscriptions);
                }

                subscriptions.Add(new Subscription() { @event = @event, @delegate = @delegate, Remove = Remove });
            }
        }

        #endregion

        #region Unregister

        /// <summary>
        /// You should not call this, only CoreEvent should be calling this.
        /// </summary>
        /// <param name="event"></param>
        /// <param name="delegate"></param>
        [Obsolete("This function is only meant to be called by the CoreEvent class. Use CoreEvent.RemoveListener instead.")]
        public void Unregister(CoreEventBase @event, Delegate @delegate)
        {

#if THREAD_SAFE
            lock (registeringObject)
#endif
            {
                if (registeringObject == null)
                    throw new NullReferenceException();

                List<Subscription> subscriptions;
                if (!listeners.TryGetValue(registeringObject, out subscriptions))
                    return;

                for (int i = subscriptions.Count; --i >= 0;)
                {
                    if ((subscriptions[i].@event == @event) && (subscriptions[i].@delegate == @delegate))
                    {
                        subscriptions[i].Remove();
                        subscriptions.RemoveAt(i);

                        if (subscriptions.Count < 1)
                        {
                            listeners.Remove(registeringObject);
                        }

                        break;
                    }
                }
            }
        }

        private class NoContainerRegistering : IDisposable
        {
            private readonly CoreEventSystem eventSystem;
            public NoContainerRegistering(CoreEventSystem s)
            {
                eventSystem = s;
            }

            public void Dispose()
            {
                eventSystem.EndRegistering();
            }
        }

        private NoContainerRegistering noContainerRegistering;

        public IDisposable SetRegisteringContext(object context)
        {
            registeringObject = context ?? throw new NullReferenceException();
            noContainerRegistering = noContainerRegistering ?? new NoContainerRegistering(this);
            return noContainerRegistering;
        }

        public TeventContainer RegisterContext<TeventContainer>(object context) where TeventContainer : CoreEventContainer
        {
            registeringObject = context ?? throw new NullReferenceException();
            return Get<TeventContainer>();
        }

        public TeventContainer Get<TeventContainer>() where TeventContainer : CoreEventContainer
        {
            if(eventContainers.TryGetValue(typeof(TeventContainer), out var container))
            {
                return container as TeventContainer;
            }
            return null;
        }

        public CoreEventContainer Get(Type containerType) 
        {
            if (eventContainers.TryGetValue(containerType, out var container))
            {
                return container;
            }
            return null;
        }

        public void RemoveFromAllEvents(object context)
        {
            if (context == null)
                throw new NullReferenceException();

            if (!listeners.TryGetValue(context, out var subscriptions))
                return;

            for (int i = subscriptions.Count; --i >= 0;)
            {
                subscriptions[i].Remove();
            }

            subscriptions.Clear();
            listeners.Remove(context);
        }

        public void UnregisterFromAllEvents(object context)
        {
            RemoveFromAllEvents(context);
        }

        #endregion

        #region Context
        /*
        public class RegisteringContext : IDisposable
        {
            private readonly CoreEventSystem system;
            public RegisteringContext(CoreEventSystem system, object registeringObject)
            {
                this.system = system;
                system.registeringObject = registeringObject ?? throw new NullReferenceException();
            }

            public static implicit operator CoreeventContainer(RegisteringContext reg)
            {
                return reg.system.eventContainer;
            }

            public void Dispose()
            {
                system.ResetContext();
            }

            public bool Register(string eventName, object target, MethodInfo methodInfo)
            {
                return system.Register(eventName, target, methodInfo);
            }

            public TeventContainer Events<TeventContainer>() where TeventContainer : CoreeventContainer
            {
                return system.Get<TeventContainer>();
            }
        }

        private RegisteringContext RegisterContextInternal(object context)
        {
            if (registeringObject != null)
                throw new Exception("Registering context is not null, don't nest registering contextes or... add a stack and refacto the whole thing ^^");

            return new RegisteringContext(this, context);
        }

        public RegisteringContext Context(object context)
        {
            return RegisterContextInternal(context);
        }

        public void ResetContext()
        {
            registeringObject = null;
        }
        */
        #endregion
        

        public void EndRegistering()
        {
            registeringObject = null;
        }

        public virtual void OnAllServicesReady()
        {
        }
    }
}
