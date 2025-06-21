using UnityEngine;
using UnityEngine.InputSystem;

namespace _1.Scripts.Entity.Scripts.Player.StateMachineScripts.States.Ground
{
    public class IdleState : GroundState
    {
        public IdleState(PlayerStateMachine machine) : base(machine)
        {
        }
        
        public override void Enter()
        {
            base.Enter();
            StartAnimation(stateMachine.Player.AnimationData.IdleParameterHash);
        }

        public override void Exit()
        {
            base.Exit();
            StopAnimation(stateMachine.Player.AnimationData.IdleParameterHash);
        }
        
        public override void Update()
        {
            base.Update();

            if (stateMachine.MovementDirection == Vector2.zero) return;
            stateMachine.ChangeState(stateMachine.WalkState);
        }
        
        protected override void OnCrouchStarted(InputAction.CallbackContext context)
        {
            base.OnCrouchStarted(context);
            stateMachine.ChangeState(stateMachine.CrouchState);
        }
    }
}