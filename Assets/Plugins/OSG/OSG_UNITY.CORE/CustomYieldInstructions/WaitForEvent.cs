// Old Skull Games
// Bernard Barthelemy
// Thursday, June 8, 2017

using System;
using OSG.EventSystem;
using UnityEngine;

namespace OSG
{
    public class WaitForEvent<T, U> : CustomYieldInstruction, IDisposable where T : GameEvent<U>
    {
        protected T myEvent;
        protected U param;
        protected bool triggered;

        public override bool keepWaiting => !triggered;

        public WaitForEvent(T theEvent)
        {
            myEvent = theEvent;
            myEvent.AddListener(OnEventTriggered);
            ResetEvent();
        }

        public void ResetEvent()
        {
            triggered = false;
        }

        public U GetParam()
        {
            return param;
        }

        private void OnEventTriggered(U param)
        {
            this.param = param;
            triggered = true;
        }

        public void Dispose()
        {
            myEvent.RemoveListener(OnEventTriggered);
        }
    }

    public class WaitForEvent<TP> : CustomYieldInstruction, IDisposable 
    {
        private GameEvent<TP> myEvent;
        private bool triggered;
        private Action<TP> myAction;

        public override bool keepWaiting => !triggered;

        public WaitForEvent(GameEvent<TP> theEvent,  Action<TP> parameterAction = null)
        {
            SetParameterAction(parameterAction);
            myEvent = theEvent;
            myEvent.AddListener(OnEventTriggered);
            ResetEvent();
        }

        public void SetParameterAction(Action<TP> parameterAction)
        {
            myAction = parameterAction;
        }

        public void ResetEvent()
        {
            triggered = false;
        }

        public void CancelWait()
        {
            triggered = true;
        }

        private void OnEventTriggered(TP evParam)
        {
            myAction?.Invoke(evParam);
            triggered = true;
        }

        public void Dispose()
        {
            myEvent.RemoveListener(OnEventTriggered);
        }
    }

    public class WaitForEvent : CustomYieldInstruction, IDisposable 
    {
        private GameEvent myEvent;
        private bool triggered;
        public override bool keepWaiting => !triggered;

        public WaitForEvent(GameEvent theEvent)
        {
            myEvent = theEvent;
            myEvent.AddListener(OnEventTriggered);
            ResetEvent();
        }

        public void ResetEvent()
        {
            triggered = false;
        }

        public void CancelWait()
        {
            triggered = true;
        }

        private void OnEventTriggered()
        {
            triggered = true;
        }

        public void Dispose()
        {
            myEvent.RemoveListener(OnEventTriggered);
        }
    }
}