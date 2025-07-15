using System.Threading;
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
            
            if (staminaCTS != null) { staminaCTS?.Cancel(); staminaCTS?.Dispose(); }
            staminaCTS = new CancellationTokenSource();
            _ = RecoverStamina_Async(playerCondition.StatData.recoverRateOfStamina_Idle * playerCondition.StatData.interval,
                playerCondition.StatData.interval, staminaCTS.Token);
        }

        public override void Exit()
        {
            base.Exit();
            
            staminaCTS?.Cancel(); staminaCTS?.Dispose(); staminaCTS = null;
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