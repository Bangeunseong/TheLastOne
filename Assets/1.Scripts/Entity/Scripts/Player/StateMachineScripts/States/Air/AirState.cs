using _1.Scripts.Weapon.Scripts;
using _1.Scripts.Weapon.Scripts.Grenade;
using _1.Scripts.Weapon.Scripts.Guns;
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
            
            // Stop FootStep Sound Coroutine
            if (footStepCoroutine != null) { stateMachine.Player.StopCoroutine(footStepCoroutine); footStepCoroutine = null; }
            
            // Stop Reload Coroutine
            if (reloadCoroutine == null) return;
            stateMachine.Player.StopCoroutine(reloadCoroutine); 
            switch (playerCondition.Weapons[playerCondition.EquippedWeaponIndex])
            {
                case Gun gun:
                    gun.IsReloading = false;
                    break;
                case GrenadeLauncher grenadeLauncher:
                    grenadeLauncher.IsReloading = false;
                    break;
            }
            reloadCoroutine = null;
        }

        public override void Exit()
        {
            base.Exit();
            StopAnimation(stateMachine.Player.AnimationData.AirParameterHash);
        }
    }
}