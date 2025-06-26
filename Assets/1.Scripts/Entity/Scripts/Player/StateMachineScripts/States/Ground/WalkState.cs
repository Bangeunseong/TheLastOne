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
            // StartAnimation(stateMachine.Player.AnimationData.WalkParameterHash);
            
            // Start Stamina Recovery Coroutine 
            if (staminaCoroutine != null) stateMachine.Player.StopCoroutine(staminaCoroutine);
            staminaCoroutine = stateMachine.Player.StartCoroutine(RecoverStamina_Coroutine(
                playerCondition.StatData.recoverRateOfStamina_Walk * playerCondition.StatData.interval,
                playerCondition.StatData.interval));
        }

        public override void Exit()
        {
            base.Exit();
            // StopAnimation(stateMachine.Player.AnimationData.WalkParameterHash);
            if (staminaCoroutine == null) return;
            stateMachine.Player.StopCoroutine(staminaCoroutine); staminaCoroutine = null;
        }

        protected override void OnCrouchStarted(InputAction.CallbackContext context)
        {
            base.OnCrouchStarted(context);
            stateMachine.ChangeState(stateMachine.CrouchState);
        }

        protected override void OnRunStarted(InputAction.CallbackContext context)
        {
            base.OnRunStarted(context);
            if (playerCondition.IsAiming) return;
            stateMachine.ChangeState(stateMachine.RunState);
        }
    }
}