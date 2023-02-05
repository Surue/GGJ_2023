using OSG.Core;
using System;
using System.Collections.Generic;

namespace Prime31.StateKit
{

	public sealed class SKStateMachine<T>
	{
	    private T _context;
		#pragma warning disable
		public event Action onStateChanged;
		#pragma warning restore

		public SKState<T> currentState { get { return _currentState; } }
		public SKState<T> previousState;
		public float elapsedTimeInState = 0f;


		private Dictionary<System.Type, SKState<T>> _states = new Dictionary<System.Type, SKState<T>>();
		private SKState<T> _currentState;


		public SKStateMachine( T context, SKState<T> initialState )
		{
			this._context = context;

			// setup our initial state
			if (initialState != null)
			{
				addState( initialState );
				_currentState = initialState;
				_currentState.begin();
			}
		}

        public void Destroy()
        {
            if (_currentState != null)
            {
                _currentState.end();
            }

            if (_states != null)
                _states.Clear();

            _states = null;
            _currentState = null;
            previousState = null;
        }


		/// <summary>
		/// adds the state to the machine
		/// </summary>
		public void addState( SKState<T> state )
		{
			state.setMachineAndContext( this, _context );

#if UNITY_EDITOR
            if (_states.ContainsKey(state.GetStateType()))
            {
                CoreDebug.LogError($"[SKStateMachine] State '{state}' already exists in StateMachine {this}");
            }
#endif

            _states[state.GetStateType()] = state;
		}

        public bool HasState<TSt>() where TSt : SKState<T>
        {
            return _states.ContainsKey(typeof(TSt));
        }


		/// <summary>
		/// ticks the state machine with the provided delta time
		/// </summary>
		public void update( float deltaTime )
		{
			elapsedTimeInState += deltaTime;
			if (_currentState != null)
			{
				_currentState.reason();
				_currentState.DoUpdate( deltaTime );
			}
		}

		public SKState<T> changeState(BaseParam myBaseParam){

			// only call end if we have a currentState
			if( _currentState != null )
				_currentState.end();

		    Type stateType = myBaseParam.stateType;

#if UNITY_EDITOR
			// do a sanity check while in the editor to ensure we have the given state in our state list
			if (stateType == null){
                CoreDebug.LogError("[SKStateMachine] State Type was not defined in your ParamForState");
			}
			if( !_states.ContainsKey( stateType ) )
			{
				var error = "[SKStateMachine] " + GetType() + ": state " + stateType + " does not exist. Did you forget to add it by calling addState?";
                CoreDebug.LogError( error );
				throw new Exception( error );
			}
#endif

			// swap states and call begin
			previousState = _currentState;
			_currentState = _states[stateType];

			_currentState.baseParam = myBaseParam;
			_currentState.begin();
			elapsedTimeInState = 0f;

			// fire the changed event if we have a listener
			if( onStateChanged != null )
				onStateChanged();
			
			return _currentState;
		} 


		/// <summary>
		/// changes the current state
		/// </summary>
		public R changeState<R>() where R : SKState<T>
		{
			// avoid changing to the same state
			var newType = typeof( R );
			if( _currentState != null && _currentState.GetStateType() == newType )
				return _currentState as R;

			// only call end if we have a currentState
			if( _currentState != null )
				_currentState.end();

#if UNITY_EDITOR
			// do a sanity check while in the editor to ensure we have the given state in our state list
			if( !_states.ContainsKey( newType ) )
			{
				var error = "[SKStateMachine] " + GetType() + ": state " + newType + " does not exist. Did you forget to add it by calling addState?";
                CoreDebug.LogError( error );
				throw new Exception( error );
			}
#endif

			// swap states and call begin
			previousState = _currentState;
			_currentState = _states[newType];
			_currentState.begin();
			elapsedTimeInState = 0f;

			// fire the changed event if we have a listener
			if( onStateChanged != null )
				onStateChanged();

			
			return _currentState as R;
		}

//		public R changeState<R>(R newState) where R : SKState<T>{
//			// only call end if we have a currentState
//			if( _currentState != null )
//				_currentState.end();
//			
//			// swap states and call begin
//			previousState = _currentState;
//			_currentState = newState;
//			_currentState.begin();
//			elapsedTimeInState = 0f;
//
//			// fire the changed event if we have a listener
//			if( onStateChanged != null )
//				onStateChanged();
//
//			return newState;
//		}

	}
}