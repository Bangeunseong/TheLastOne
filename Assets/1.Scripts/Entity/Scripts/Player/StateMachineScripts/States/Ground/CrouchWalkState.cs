using System.Threading;
using UnityEngine.InputSystem;

namespace _1.Scripts.Entity.Scripts.Player.StateMachineScripts.States.Ground
{
    public class CrouchWalkState : GroundState
    {
        public CrouchWalkState(PlayerStateMachine machine) : base(machine)
        {
        }
        
        public override void Enter()
        {
            base.Enter();
            StartAnimation(stateMachine.Player.AnimationData.CrouchParameterHash);
        }

        public override void Exit()
        {
            base.Exit();
            StopAnimation(stateMachine.Player.AnimationData.CrouchParameterHash);
        }
        
        protected override void OnCrouchStarted(InputAction.CallbackContext context)
        {
            if (playerCondition.IsCrouching)
            {
                playerCondition.IsCrouching = false;
                if (crouchCTS != null) { crouchCTS.Cancel(); crouchCTS.Dispose(); }
                crouchCTS = new CancellationTokenSource();
                _ = Crouch_Async(playerCondition.IsCrouching, 0.1f, crouchCTS.Token); 
            }
            base.OnCrouchStarted(context);
            stateMachine.ChangeState(stateMachine.WalkState);
        }
    }
}