using _1.Scripts.Weapon.Scripts;
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
            if (stateMachine.Player.Weapons[stateMachine.Player.EquippedWeaponIndex] is not Gun gun) return;
            stateMachine.Player.StopCoroutine(reloadCoroutine); 
            gun.IsReloading = false;
            reloadCoroutine = null;
        }

        public override void Exit()
        {
            base.Exit();
            StopAnimation(stateMachine.Player.AnimationData.AirParameterHash);
        }
    }
}