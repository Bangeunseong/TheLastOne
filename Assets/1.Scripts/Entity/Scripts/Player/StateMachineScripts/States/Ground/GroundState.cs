using UnityEngine;
using UnityEngine.InputSystem;

namespace _1.Scripts.Entity.Scripts.Player.StateMachineScripts.States.Ground
{
    public class GroundState : BaseState
    {
        public GroundState(PlayerStateMachine machine) : base(machine)
        {
        }
        
        public override void Enter()
        {
            base.Enter();
            StartAnimation(stateMachine.Player.AnimationData.GroundParameterHash);
        }
        
        public override void Exit()
        {
            base.Exit();
            StopAnimation(stateMachine.Player.AnimationData.GroundParameterHash);
        }
        
        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            if (!stateMachine.Player.PlayerGravity.IsGrounded &&
                stateMachine.Player.Rigidbody.velocity.y < Physics.gravity.y * Time.fixedUnscaledDeltaTime)
            {
                stateMachine.ChangeState(stateMachine.FallState);
            }
        }

        protected override void OnMoveCanceled(InputAction.CallbackContext context)
        {
            if (stateMachine.MovementDirection == Vector2.zero) return;
            
            base.OnMoveCanceled(context);
            if (stateMachine.CurrentState is CrouchWalkState) stateMachine.ChangeState(stateMachine.CrouchState);
            else stateMachine.ChangeState(stateMachine.IdleState);
        }

        protected override void OnJumpStarted(InputAction.CallbackContext context)
        {
            base.OnJumpStarted(context);
            stateMachine.ChangeState(stateMachine.JumpState);
        }

        protected override void OnReloadStarted(InputAction.CallbackContext context)
        {
            base.OnReloadStarted(context);
            playerCondition.TryStartReload();
        }
    }
}