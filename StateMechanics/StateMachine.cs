using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace StateMechanics
{
    public sealed class StateMachine<T> where T : class
    {
        public State<T> CurrentState => currentState;
        
        private State<T> currentState;
        
        private Action<State<T>> stateSwitch;

        private T stateMechanic = null;
        private Dictionary<Type, State<T>> stateLookUp = new Dictionary<Type, State<T>>();

        public StateMachine(T stateMechanic)
        {
            this.stateMechanic = stateMechanic;
        }

        public void SetMechanic(T stateMechanic)
        {
            this.stateMechanic = stateMechanic;
        }

        public void CleanUp()
        {
            currentState.CleanUp();
        }

        public void Update()
        {
            currentState.Update();
        }

        public void RegisterSwitchStateCallback(Action<State<T>> callback)
        {
            stateSwitch += callback;
        }

        public void UnregisterSwitchStateCallback(Action<State<T>> callback)
        {
            stateSwitch -= callback;
        }

        /// <summary>
        /// Automatically create states from inner classes
        /// </summary>
        /// <typeparam name="S">The Type of the derived Mechanic class</typeparam>
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

        public State<T>[] GetStates<S>() where S : State<T>
        {
            return stateLookUp.Where(kvp => typeof(S).IsAssignableFrom(kvp.Key)).Select(kvp => kvp.Value).ToArray();
        }

        public S GetState<S>() where S : State<T>
        {
            return stateLookUp.Where(kvp => typeof(S).IsAssignableFrom(kvp.Key)).Select(kvp => kvp.Value).FirstOrDefault() as S;
        }

        public void AddState<S>() where S : State<T>
        {
            Type stateType = typeof(S);
            State<T> newState = (State<T>)Activator.CreateInstance(stateType);
            stateLookUp.Add(stateType, newState);
            newState.SetMechanic(stateMechanic);
            newState.SetStateMachine(this);
            newState.Init();
        }

        public Type GetCurrentStateType()
        {
            return currentState.GetType();
        }

        public bool IsCurrentState<I>()
        {
            bool match = false;
            Type t = currentState.GetType();

            if (typeof(I) == t)
                match = true;

            return match;
        }

        public void SwitchTo<S>()
        {
            Type StateType = typeof(S);

            if (stateLookUp.TryGetValue(StateType, out State<T> nextState))
            {
                ProcessNextState(nextState);
            }
        }


        public void SwitchTo(State<T> nextState)
        {
            if (stateLookUp.ContainsValue(nextState))
            {
                ProcessNextState(nextState);
            }
        }

        private void AddStateType(Type stateType)
        {
            State<T> newState = (State<T>)Activator.CreateInstance(stateType);
            stateLookUp.Add(stateType, newState);
            newState.SetMechanic(stateMechanic);
            newState.SetStateMachine(this);
            newState.Init();
        }


        private void ProcessNextState(State<T> nextState)
        {
            stateSwitch?.Invoke(nextState);

            currentState?.Exit();
            currentState = nextState;
            currentState.Enter();
        }
    }

}