using System.Collections;
using _1.Scripts.Weapon.Scripts;
using _1.Scripts.Weapon.Scripts.Grenade;
using _1.Scripts.Weapon.Scripts.Guns;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _1.Scripts.Entity.Scripts.Player.StateMachineScripts.States.Ground
{
    public class GroundState : BaseState
    {
        public GroundState(PlayerStateMachine machine) : base(machine)
        {
        }
        
        public override void Enter()
        {
            base.Enter();
            StartAnimation(stateMachine.Player.AnimationData.GroundParameterHash);
        }
        
        public override void Exit()
        {
            base.Exit();
            StopAnimation(stateMachine.Player.AnimationData.GroundParameterHash);
        }
        
        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            if (!stateMachine.Player.PlayerGravity.IsGrounded &&
                stateMachine.Player.Controller.velocity.y < Physics.gravity.y * Time.deltaTime)
            {
                stateMachine.ChangeState(stateMachine.FallState);
            }
        }

        protected override void OnMoveCanceled(InputAction.CallbackContext context)
        {
            if (stateMachine.MovementDirection == Vector2.zero) return;
            
            base.OnMoveCanceled(context);
            if (stateMachine.CurrentState is CrouchWalkState) stateMachine.ChangeState(stateMachine.CrouchState);
            else stateMachine.ChangeState(stateMachine.IdleState);
        }

        protected override void OnJumpStarted(InputAction.CallbackContext context)
        {
            base.OnJumpStarted(context);
            stateMachine.ChangeState(stateMachine.JumpState);
        }

        protected override void OnReloadStarted(InputAction.CallbackContext context)
        {
            base.OnReloadStarted(context);
            if (playerCondition.EquippedWeaponIndex < 0) return;

            if (playerCondition.Weapons[playerCondition.EquippedWeaponIndex] is Gun gun)
            {
                if (!gun.IsReadyToReload) return;
                if (reloadCoroutine != null) { stateMachine.Player.StopCoroutine(reloadCoroutine); gun.IsReloading = false; }
                reloadCoroutine = stateMachine.Player.StartCoroutine(Reload_Coroutine(gun.GunData.GunStat.ReloadTime));
            } else if (playerCondition.Weapons[playerCondition.EquippedWeaponIndex] is GrenadeLauncher grenadeLauncher)
            {
                if (!grenadeLauncher.IsReadyToReload) return;
                if (reloadCoroutine != null) { stateMachine.Player.StopCoroutine(reloadCoroutine); grenadeLauncher.IsReloading = false; }
                reloadCoroutine = stateMachine.Player.StartCoroutine(Reload_Coroutine(grenadeLauncher.GrenadeData.GrenadeStat.ReloadTime));
            }
        }
    }
}