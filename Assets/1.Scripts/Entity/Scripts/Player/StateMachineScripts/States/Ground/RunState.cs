using UnityEngine.InputSystem;

namespace _1.Scripts.Entity.Scripts.Player.StateMachineScripts.States.Ground
{
    public class RunState : GroundState
    {
        public RunState(PlayerStateMachine machine) : base(machine)
        {
        }
        
        public override void Enter()
        {
            stateMachine.MovementSpeedModifier = playerCondition.RunSpeedModifier;
            base.Enter();
            // StartAnimation(stateMachine.Player.AnimationData.RunParameterHash);
            
            if (staminaCoroutine != null) stateMachine.Player.StopCoroutine(staminaCoroutine);
            staminaCoroutine = stateMachine.Player.StartCoroutine(ConsumeStamina_Coroutine(
                playerCondition.StatData.consumeRateOfStamina * playerCondition.StatData.interval,
                playerCondition.StatData.interval));
        }

        public override void Update()
        {
            base.Update();
            if (playerCondition.CurrentStamina <= 0){ stateMachine.ChangeState(stateMachine.WalkState); }
        }

        public override void Exit()
        {
            base.Exit();
            // StopAnimation(stateMachine.Player.AnimationData.RunParameterHash);
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
            stateMachine.ChangeState(stateMachine.WalkState);
        }

        protected override void OnAimStarted(InputAction.CallbackContext context)
        {
            base.OnAimStarted(context);
            stateMachine.ChangeState(stateMachine.WalkState);
        }
    }
}