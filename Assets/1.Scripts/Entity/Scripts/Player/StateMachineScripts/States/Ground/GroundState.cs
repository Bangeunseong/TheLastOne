using _1.Scripts.UI.Inventory;
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
            coreManager.uiManager.HideUI<InventoryUI>();
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            if (!stateMachine.Player.PlayerGravity.IsGrounded &&
                stateMachine.Player.Controller.velocity.y < Physics.gravity.y * Time.unscaledDeltaTime)
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

        protected override void OnInventoryToggled(InputAction.CallbackContext context)
        {
            base.OnInventoryToggled(context);
            
            var ui = coreManager.uiManager.GetUI<InventoryUI>();
            if (ui.gameObject.activeInHierarchy) coreManager.uiManager.HideUI<InventoryUI>();
            else coreManager.uiManager.ShowUI<InventoryUI>();
        }
    }
}