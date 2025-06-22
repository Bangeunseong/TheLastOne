using UnityEngine;
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
            StartAnimation(stateMachine.Player.AnimationData.CrouchWalkParameterHash);
        }

        public override void Exit()
        {
            base.Exit();
            StopAnimation(stateMachine.Player.AnimationData.CrouchWalkParameterHash);
        }
        
        protected override void OnCrouchStarted(InputAction.CallbackContext context)
        {
            base.OnCrouchStarted(context);
            stateMachine.ChangeState(stateMachine.WalkState);
        }
    }
}