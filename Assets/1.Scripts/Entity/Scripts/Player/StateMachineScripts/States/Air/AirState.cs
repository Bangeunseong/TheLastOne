using System.Threading;

namespace _1.Scripts.Entity.Scripts.Player.StateMachineScripts.States.Air
{
    public class AirState : BaseState
    {
        public AirState(PlayerStateMachine machine) : base(machine)
        {
        }
        
        public override void Enter()
        {
            base.Enter();
            StartAnimation(stateMachine.Player.AnimationData.AirParameterHash);
            
            // Cancel Crouch
            playerCondition.OnCrouch(false, 0.1f);
            
            // Stop Reload Coroutine
            playerCondition.TryCancelReload();
        }

        public override void Exit()
        {
            base.Exit();
            StopAnimation(stateMachine.Player.AnimationData.AirParameterHash);
        }
    }
}