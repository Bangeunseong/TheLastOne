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
            base.Enter();
            // StartAnimation(stateMachine.Player.AnimationData.CrouchParameterHash);
        }

        public override void Exit()
        {
            base.Exit();
            // StopAnimation(stateMachine.Player.AnimationData.CrouchParameterHash);
        }

        protected override void OnCrouchStarted(InputAction.CallbackContext context)
        {
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