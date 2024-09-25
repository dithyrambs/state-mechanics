using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace StateMechanics
{
    /// <summary>
    /// A scalable state-machine that requires minimal boiler-plate. States are accessed by type,
    /// similar to how components are accessed in Unity. Users can either populate states from 
    /// inner classes, or add them manually by type.
    /// </summary>
    public sealed class StateMachine<T> where T : class
    {
        /// <summary>
        /// Event invoked when the state is switched to a new state. eg. can print to console for debugging.
        /// </summary>
        private event Action<State<T>> StateSwitched;

        /// <summary>
        /// Event invoked if the state to be switched to cannot be found. eg. can use to throw errors.
        /// </summary>
        private event Action<Type> InvalidStateSwitchAttempt;

        private readonly Dictionary<Type, State<T>> _stateLookUp = new();

        private readonly T _stateMechanic = null;

        private State<T> _currentState;

        /// <summary>
        /// Get the currently running state of the state machine.
        /// </summary>
        public State<T> CurrentState => this._currentState;

        /// <summary>
        /// Get the type of the currently running state of the state machine.
        /// </summary>
        public Type currentStateType => this._currentState.GetType();

        /// <summary>
        /// On construction, pass in the client operating this machine.
        /// </summary>
        public StateMachine(T stateMechanic)
        {
            this._stateMechanic = stateMechanic;
        }

        /// <summary>
        /// Call when the state machine is destroyed.
        /// </summary>
        public void CleanUp()
        {
            foreach (KeyValuePair<Type, State<T>> entry in _stateLookUp)
            {
                entry.Value.CleanUp();
            }
        }

        /// <summary>
        /// Call every frame/tick.
        /// </summary>
        public void Update()
        {
            this._currentState.Update();
        }

        /// <summary>
        /// Register a callback to be invoked every time the state machine switches states.
        /// </summary>
        public void RegisterSwitchStateCallback(Action<State<T>> callback)
        {
            this.StateSwitched += callback;
        }

        /// <summary>
        /// Unregister a callback that was invoked every time the state machine switches states.
        /// </summary>
        public void UnregisterSwitchStateCallback(Action<State<T>> callback)
        {
            this.StateSwitched -= callback;
        }


        /// <summary>
        /// Register a callback to be invoked every time the state machine switch fails.
        /// </summary>
        public void RegisterInvalidStateSwitchAttemptCallback(Action<Type> callback)
        {
            this.InvalidStateSwitchAttempt += callback;
        }

        /// <summary>
        /// Unregister a callback that was invoked every time the state machine switch false.
        /// </summary>
        public void UnregisterInvalidStateSwitchAttemptCallback(Action<Type> callback)
        {
            this.InvalidStateSwitchAttempt -= callback;
        }

        /// <summary>
        /// Automatically populate our states from the inner classes of type S.
        /// </summary>
        public void SetStatesFromInnerClasses<S>()
        {
            Type[] myTypeArray;
            Type type = typeof(S);

            myTypeArray = type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var t in myTypeArray)
            {
                if (t.IsSubclassOf(typeof(State<T>)) && !t.IsAbstract)
                    AddStateType(t);
            }
        }

        /// <summary>
        ///  Retrieve array of states in our machine of type S, where S inherits from State<T>.
        /// </summary>
        public State<T>[] GetStates<S>() where S : State<T>
        {
            return this._stateLookUp.Where(kvp => typeof(S).IsAssignableFrom(kvp.Key)).Select(kvp => kvp.Value).ToArray();
        }

        /// <summary>
        /// Retrieve state of type S, where S inherits from State.
        /// </summary>
        public S GetState<S>() where S : State<T>
        {
            return this._stateLookUp.Where(kvp => typeof(S).IsAssignableFrom(kvp.Key)).Select(kvp => kvp.Value).FirstOrDefault() as S;
        }

        /// <summary>
        /// Add a state to our state machine of type S, where S inherits from State.
        /// </summary>
        public void AddState<S>() where S : State<T>
        {
            Type stateType = typeof(S);
            State<T> newState = (State<T>)Activator.CreateInstance(stateType);
            this._stateLookUp.Add(stateType, newState);
            newState.InitFields(this, this._stateMechanic);
            newState.Init();
        }

        /// <summary>
        /// Returns true if the current state is type S, where S inherits from State.
        /// </summary>
        public bool IsCurrentState<S>() where S : State<T>
        {
            bool match = false;
            Type t = this._currentState.GetType();

            if (typeof(S) == t)
                match = true;

            return match;
        }

        /// <summary>
        /// Switch to the state of type S, where S inherits from State. If the state cannot be found, InvalidStateSwitchAttempt is invoked.
        /// </summary>
        public void SwitchTo<S>() where S : State<T>
        {
            Type StateType = typeof(S);

            if (this._stateLookUp.TryGetValue(StateType, out State<T> nextState))
            {
                ProcessNextState(nextState);
            }
            else
            {
                this.InvalidStateSwitchAttempt?.Invoke(typeof(S));
            }
        }

        /// <summary>
        /// Switch to state of type State<T>. If the state cannot be found, InvalidStateSwitchAttempt is invoked.
        /// </summary>
        public void SwitchTo(State<T> nextState)
        {
            if (this._stateLookUp.ContainsValue(nextState))
            {
                ProcessNextState(nextState);
            }
            else
            {
                this.InvalidStateSwitchAttempt?.Invoke(nextState.GetType());
            }
        }

        private void AddStateType(Type stateType)
        {
            State<T> newState = (State<T>)Activator.CreateInstance(stateType);
            this._stateLookUp.Add(stateType, newState);
            newState.InitFields(this, this._stateMechanic);
            newState.Init();
        }


        private void ProcessNextState(State<T> nextState)
        {
            this.StateSwitched?.Invoke(nextState);

            this._currentState?.Exit();
            this._currentState = nextState;
            this._currentState.Enter();
        }
    }

}
