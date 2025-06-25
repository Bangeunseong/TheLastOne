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
            stateMachine.MovementSpeedModifier = 0f;
            base.Enter();
            // StartAnimation(stateMachine.Player.AnimationData.IdleParameterHash);
            if (staminaCoroutine != null) stateMachine.Player.StopCoroutine(staminaCoroutine);
            if(stateMachine.Player == null) Debug.Log("Player is null!");
            staminaCoroutine = stateMachine.Player.StartCoroutine(RecoverStamina_Coroutine(
                playerCondition.StatData.recoverRateOfStamina_Idle * playerCondition.StatData.interval,
                playerCondition.StatData.interval));
        }

        public override void Exit()
        {
            base.Exit();
            // StopAnimation(stateMachine.Player.AnimationData.IdleParameterHash);
            if (staminaCoroutine == null) return;
            stateMachine.Player.StopCoroutine(staminaCoroutine); staminaCoroutine = null;
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