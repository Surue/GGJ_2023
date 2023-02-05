// Old Skull Games
// Pierre Planeau
// Thursday, June 28, 2018

using UnityEngine;
using System;
using OSG.Core.EventSystem;
using OSG.Services;

namespace OSG
{
    public class WaitForCoreEvent<T> : CustomYieldInstruction, IDisposable where T : CoreEvent
    {
        private T myEvent;
        private bool triggered;
        private object context;

        public override bool keepWaiting
        {
            get { return !triggered; }
        }

        public WaitForCoreEvent(T theEvent, object context)
        {
            myEvent = theEvent;
            this.context = context;
            ServiceRef<IEventSystem> eventSystem = new ServiceRef<IEventSystem>();
            using (eventSystem.Value.RegisterContext<CoreEventContainer>(context))
            {
                myEvent.AddListener(OnEventTriggered);
            }
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

    public class WaitForCoreEvent<T, U> : CustomYieldInstruction, IDisposable where T : CoreEvent<U>
    {
        protected T myEvent;
        protected U param;
        protected bool triggered;
        private object context;

        public override bool keepWaiting
        {
            get
            {
                return !triggered;
            }
        }

        public WaitForCoreEvent(T theEvent, object context)
        {
            myEvent = theEvent;
            this.context = context;
            ServiceRef<IEventSystem> system = new ServiceRef<IEventSystem>();
            using (system.Value.RegisterContext<CoreEventContainer>(context))
            {
                myEvent.AddListener(OnEventTriggered);
            }

            ResetEvent();
        }

        public void ResetEvent()
        {
            triggered = false;
        }

        /// <summary>
        /// Returns the param given to the event the first time it got triggered.
        /// </summary>
        /// <returns></returns>
        public U GetParam()
        {
            return param;
        }

        private void OnEventTriggered(U param)
        {
            if (triggered)
                return;

            this.param = param;
            triggered = true;
        }

        public void Dispose()
        {
            myEvent.RemoveListener(OnEventTriggered);
        }
    }
}
