using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;

namespace _1.Scripts.Entity.Scripts.Player.StateMachineScripts.States.Air
{
    public class FallState : AirState
    {
        public FallState(PlayerStateMachine machine) : base(machine)
        {
        }
        
        public override void Enter()
        {
            base.Enter();
            StartAnimation(stateMachine.Player.AnimationData.FallParameterHash);
        }

        public override void Exit()
        {
            base.Exit();
            StopAnimation(stateMachine.Player.AnimationData.FallParameterHash);
            CoreManager.Instance.soundManager.PlaySFX(SfxType.PlayerLand, stateMachine.Player.transform.position, -1);
        }

        public override void Update()
        {
            base.Update();
            if (stateMachine.Player.PlayerGravity.IsGrounded) 
                stateMachine.ChangeState(stateMachine.IdleState);
        }
    }
}