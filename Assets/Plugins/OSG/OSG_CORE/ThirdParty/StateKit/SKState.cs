using OSG.Core;
using System;

namespace Prime31.StateKit
{
    public abstract class SKState<T>
    {
        protected int _mecanimStateHash;
        protected SKStateMachine<T> _machine;
        protected T _context;
        public BaseParam baseParam;

        public SKState()
        {
        }


        /// <summary>
        /// constructor that takes the mecanim state name as a string
        /// </summary>
        public SKState(string mecanimStateName) : this(mecanimStateName.GetHashCode()) // Animator.StringToHash(mecanimStateName)
        {
        }


        /// <summary>
        /// constructor that takes the mecanim state hash
        /// </summary>
        public SKState(int mecanimStateHash)
        {
            _mecanimStateHash = mecanimStateHash;
        }


        internal void setMachineAndContext(SKStateMachine<T> machine, T context)
        {
            _machine = machine;
            _context = context;
            onInitialized();
        }


        /// <summary>
        /// called directly after the machine and context are set allowing the state to do any required setup
        /// </summary>
        public virtual void onInitialized()
        {
        }


        public virtual void begin()
        {
        }


        public virtual void reason()
        {
        }


        public abstract void update(float deltaTime);

        public virtual void DoUpdate(float deltaTime)
        {
            update(deltaTime);
        }

        public virtual void end()
        {
        }

        /// <summary>
        /// Whether the State can be interrupted anywhen or not.
        /// Ex: an Idle State is interuptible.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsInteruptible()
        {
            return false;
        }

        public virtual Type GetStateType()
        {
            return GetType();
        }
    }
}
