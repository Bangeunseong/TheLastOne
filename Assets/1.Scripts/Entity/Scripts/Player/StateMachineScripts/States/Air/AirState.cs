using UnityEngine;

namespace _1.Scripts.Entity.Scripts.Player.StateMachineScripts.States.Air
{
    public class AirState : BaseState
    {
        public AirState(PlayerStateMachine machine) : base(machine)
        {
        }
        
        public override void Enter()
        {
            base.Enter();
            StartAnimation(stateMachine.Player.AnimationData.AirParameterHash);
            if (reloadCoroutine == null) return;
            stateMachine.Player.StopCoroutine(reloadCoroutine); 
            stateMachine.Player.Guns[stateMachine.Player.EquippedGunIndex].IsReloading = false;
            reloadCoroutine = null;
        }

        public override void Exit()
        {
            base.Exit();
            StopAnimation(stateMachine.Player.AnimationData.AirParameterHash);
        }
    }
}