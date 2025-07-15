using System.Threading;
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
            
            // Start Stamina Recovery Coroutine 
            if (staminaCTS != null) { staminaCTS?.Cancel(); staminaCTS?.Dispose(); }
            staminaCTS = new CancellationTokenSource();
            _ = RecoverStamina_Async(playerCondition.StatData.recoverRateOfStamina_Walk * playerCondition.StatData.interval,
                playerCondition.StatData.interval, staminaCTS.Token);
        }

        public override void Exit()
        {
            base.Exit();
            
            staminaCTS?.Cancel(); staminaCTS?.Dispose(); staminaCTS = null;
        }

        protected override void OnCrouchStarted(InputAction.CallbackContext context)
        {
            base.OnCrouchStarted(context);
            stateMachine.ChangeState(stateMachine.CrouchState);
        }

        protected override void OnRunStarted(InputAction.CallbackContext context)
        {
            base.OnRunStarted(context);
            if (playerCondition.IsAiming || playerCondition.IsAttacking) return;
            stateMachine.ChangeState(stateMachine.RunState);
        }
    }
}