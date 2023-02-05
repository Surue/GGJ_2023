// Old Skull Games
// Pierre Planeau
// Monday, May 21, 2018

#if UNITY_EDITOR
#define CANLOG
#endif

using System;

namespace OSG.Core.EventSystem
{
    public abstract class CoreEventBase
    {
        protected IEventSystem eventSystem;
#if CANLOG
        protected Action<object> onLog;
        public void RegisterLogger(Action<object> logAction)
        {
            onLog = logAction;
        }
#endif
        public void SetEventSystem(CoreEventSystem system)
        {
            eventSystem = system;
        }
    }

    public interface IEmitable
    {
        Type PayloadType { get; }
#if CANLOG
        void RegisterLogger(Action<object> logAction);
#endif
    }

    public delegate void CoreAction();
    public delegate void CoreAction<in T0>(T0 arg0);

    /// <summary>
    /// Defines the base type for a GameEvent without payload
    /// \details To make a derived type usable, you have to make it `[Serializable]`
    /// </summary>
    [Serializable]
    public class CoreEvent : CoreEventBase, IEmitable
    {

        protected event CoreAction ActionEvent;

        /// <summary>
        /// Triggers the GameEvent
        /// </summary>
        /// <param name="p"></param>
        public void Invoke()
        {
#if CANLOG
            onLog?.Invoke(null);
#endif
            ActionEvent?.Invoke();
        }

        public Type PayloadType => typeof(void);


#pragma warning disable 618 // shut up, I AM CoreEvent
        /// <summary>
        /// Adds an action to be called when the event is emitted.
        /// Use only when in a 'using (CoreEventSystem.Context(context)) { }'.
        /// </summary>
        /// <param name="action"></param>
        public void AddListener(CoreAction action)
        {
            eventSystem.Register(this, action);
        }

        /// <summary>
        /// Remove a specific Listener.
        /// Use only when in a 'using (CoreEventSystem.Context(context)) { }'.
        /// </summary>
        /// <param name="action"></param>
        public void RemoveListener(CoreAction action)
        {
            eventSystem.Unregister(this, action);
        }
#pragma warning restore 618

        /// <summary>
        /// Don't call it yourself, this is meant for CoreEventSystem's use only.
        /// </summary>
        /// <param name="action"></param>
        [Obsolete("This function is only meant to be called by the GameEventSystem. Use AddListener instead.")]
        public void DirectAddListener(CoreAction action)
        {
            ActionEvent += action;
        }

        /// <summary>
        /// Don't call it yourself, this is meant for GameEventSystem's use only.
        /// </summary>
        /// <param name="action"></param>
        [Obsolete("This function is only meant to be called by the GameEventSystem. Use RemoveListener instead.")]
        public void DirectRemoveListener(CoreAction action)
        {
            ActionEvent -= action;
        }
    }

    /// <summary>
    /// Base class for Game Events with a parameter of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class CoreEvent<T> : CoreEventBase, IEmitable
    {
        protected event CoreAction<T> ActionEvent;

        /// <summary>
        /// Triggers the GameEvent
        /// </summary>
        /// <param name="p"></param>
        public void Invoke(T p)
        {
#if CANLOG
            onLog?.Invoke(p);
#endif
            ActionEvent?.Invoke(p);
        }

        public Type PayloadType => typeof(T);


#pragma warning disable 618 // shut up, I AM CoreEvent
        /// <summary>
        /// Adds an action to be called when the event is emitted.
        /// Use only when in a 'using (CoreEventSystem.Context(context)) { }'.
        /// </summary>
        /// <param name="action"></param>
        public void AddListener(CoreAction<T> action)
        {
            eventSystem.Register(this, action);
        }

        /// <summary>
        /// Remove a specific listener.
        /// Use only when in a 'using (CoreEventSystem.Context(context)) { }'.
        /// </summary>
        /// <param name="call"></param>
        public void RemoveListener(CoreAction<T> call)
        {
            eventSystem.Unregister(this, call);
        }
#pragma warning restore 618

        /// <summary>
        /// Bypasses the GameEventSystem to add a Listener (don't call it yourself, this 
        /// is meant for GameEventSystem's use only
        /// </summary>
        /// <param name="action"></param>
        [Obsolete("This function is only meant to be called by the GameEventSystem. Use AddListener instead.")]
        public void DirectAddListener(CoreAction<T> action)
        {
            ActionEvent += action;
        }

        /// <summary>
        /// Bypasses the GameEventSystem to remove a Listener (don't call it yourself, this 
        /// is meant for GameEventSystem's use only
        /// </summary>
        /// <param name="action"></param>
        [Obsolete("This function is only meant to be called by the GameEventSystem. Use Remove instead.")]
        public void DirectRemoveListener(CoreAction<T> action)
        {
            ActionEvent -= action;
        }
    }

    [Serializable] public class StringEvent : CoreEvent<string> {}
    [Serializable] public class StringArrayEvent : CoreEvent<string[]> {}
    [Serializable] public class BoolEvent : CoreEvent<bool> {}
    [Serializable] public class IntEvent : CoreEvent<int> {}
    [Serializable] public class IntArrayEvent : CoreEvent<int[]> {}
    [Serializable] public class IntPairEvent : CoreEvent<ValueTuple<int,int>> {}
    [Serializable] public class FloatEvent : CoreEvent<float> {}


}
