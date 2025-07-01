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
            playerCondition.TryCancelReload();
        }

        public override void Exit()
        {
            base.Exit();
            StopAnimation(stateMachine.Player.AnimationData.AirParameterHash);
        }
    }
}