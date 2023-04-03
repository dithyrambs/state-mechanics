namespace StateMechanics
{
    public abstract class State<T> where T : class
    {
        protected T Mechanic;

        private StateMachine<T> stateMachine;

        public void SetStateMachine(StateMachine<T> stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        public void SetMechanic(T mechanic)
        {
            this.Mechanic = mechanic;
        }

        public virtual void Init()
        {
        }

        public virtual void Enter()
        {
        }

        public virtual void Update()
        {
        }

        public virtual void Exit()
        {
        }

        public virtual void CleanUp() 
        { 
        }

        protected void SwitchTo<S>()
        {
            stateMachine.SwitchTo<S>();
        }

        protected void SwitchTo(State<T> state)
        {
            stateMachine.SwitchTo(state);
        }
    }
}