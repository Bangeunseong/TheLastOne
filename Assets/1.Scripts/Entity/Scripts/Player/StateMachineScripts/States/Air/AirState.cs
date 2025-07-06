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
            if (playerCondition.IsCrouching)
            {
                if (crouchCoroutine != null) { stateMachine.Player.StopCoroutine(crouchCoroutine); }
                crouchCoroutine =
                    stateMachine.Player.StartCoroutine(Crouch_Coroutine(playerCondition.IsCrouching = false, 0.1f));
            }
            
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