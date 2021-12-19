namespace FiniteStateMachine
{
    public interface IState
    {
        public void OnStateEnter(StateMachine stateMachine);
        public void OnStateUpdate(StateMachine stateMachine, float elapsedTime);
        public void OnStateExit(StateMachine stateMachine);
    }
}