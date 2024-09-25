namespace StateMechanics
{
    public abstract class State<T> where T : class
    {
        // The state machine that this state belongs to.
        private StateMachine<T> _stateMachine;
        
        private T _mechanic;

        /// <summary>
        /// The Mechanic is the client this is operating this state's machine.
        /// </summary>
        public T Mechanic => _mechanic;

        /// <summary>
        /// When constructing a new state, pass in its owning machine, and the client operating it.
        /// </summary>
        public void InitFields(StateMachine<T> stateMachine, T mechanic)
        {
            this._stateMachine = stateMachine;
            this._mechanic = mechanic;
        }

        /// <summary>
        /// Virtual method called when the State is constructed and added to the machine.
        /// </summary>
        public virtual void Init()
        {
        }

        /// <summary>
        /// Virtual method called when the state machine switches to this state.
        /// </summary>
        public virtual void Enter()
        {
        }

        /// <summary>
        /// Virtual method called on StateMachine.Update().
        /// </summary>
        public virtual void Update()
        {
        }

        /// <summary>
        /// Virtual method called when the StateMachine switches to another state.
        /// </summary>
        public virtual void Exit()
        {
        }

        /// <summary>
        /// Virtual method called on Statemachine.CleanUp().
        /// </summary>
        public virtual void CleanUp() 
        { 
        }

        /// <summary>
        /// Switches the state machine to a state of this type.
        /// </summary>
        protected void SwitchTo<S>() where S : State<T>
        {
            _stateMachine.SwitchTo<S>();
        }

        /// <summary>
        /// Switches the state machine to this state.
        /// </summary>
        protected void SwitchTo(State<T> state)
        {
            _stateMachine.SwitchTo(state);
        }
    }
}
