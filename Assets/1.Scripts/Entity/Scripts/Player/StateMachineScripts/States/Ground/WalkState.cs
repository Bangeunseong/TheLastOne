using UnityEngine;
using UnityEngine.InputSystem;

namespace _1.Scripts.Entity.Scripts.Player.StateMachineScripts.States.Ground
{
    public class WalkState : GroundState
    {
        public WalkState(PlayerStateMachine machine) : base(machine)
        {
        }
        
        public override void Enter()
        {
            stateMachine.MovementSpeedModifier = playerCondition.WalkSpeedModifier;
            base.Enter();
            StartAnimation(stateMachine.Player.AnimationData.WalkParameterHash);
        }

        public override void Exit()
        {
            base.Exit();
            StopAnimation(stateMachine.Player.AnimationData.WalkParameterHash);
        }

        protected override void OnCrouchStarted(InputAction.CallbackContext context)
        {
            base.OnCrouchStarted(context);
            stateMachine.ChangeState(stateMachine.CrouchState);
        }

        protected override void OnRunStarted(InputAction.CallbackContext context)
        {
            base.OnRunStarted(context);
            stateMachine.ChangeState(stateMachine.RunState);
        }
    }
}