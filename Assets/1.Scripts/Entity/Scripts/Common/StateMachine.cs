namespace _1.Scripts.Entity.Scripts.Common
{
    public interface IState
    {
        public void Enter();
        public void HandleInput();
        public void Update();
        public void PhysicsUpdate();
        public void Exit();
    }
    
    public abstract class StateMachine
    {
        public IState CurrentState { get; protected set; }

        public void ChangeState(IState newState)
        {
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState?.Enter();
        }

        public void HandleInput()
        {
            CurrentState?.HandleInput();
        }

        public void Update()
        {
            CurrentState?.Update();
        }

        public void PhysicsUpdate()
        {
            CurrentState?.PhysicsUpdate();
        }
    }
}