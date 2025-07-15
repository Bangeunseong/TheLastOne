using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _1.Scripts.Entity.Scripts.Player.StateMachineScripts.States.Ground
{
    public class CrouchState : GroundState
    {
        public CrouchState(PlayerStateMachine machine) : base(machine)
        {
        }
        
        public override void Enter()
        {
            stateMachine.MovementSpeedModifier = playerCondition.CrouchSpeedModifier;
            if (!playerCondition.IsCrouching)
            {
                playerCondition.IsCrouching = true;
                if (crouchCTS != null) { crouchCTS.Cancel(); crouchCTS.Dispose(); }
                crouchCTS = new CancellationTokenSource();
                _ = Crouch_Async(playerCondition.IsCrouching, 0.1f, crouchCTS.Token); 
            }
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
            stateMachine.ChangeState(stateMachine.IdleState);
        }
        
        public override void Update()
        {
            base.Update();

            if (stateMachine.MovementDirection == Vector2.zero) return;
            stateMachine.ChangeState(stateMachine.CrouchWalkState);
        }
    }
}