using System.Collections;
using _1.Scripts.Entity.Scripts.Common;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Interfaces;
using _1.Scripts.Interfaces.Player;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.Weapon.Scripts;
using _1.Scripts.Weapon.Scripts.Common;
using _1.Scripts.Weapon.Scripts.Grenade;
using _1.Scripts.Weapon.Scripts.Guns;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _1.Scripts.Entity.Scripts.Player.StateMachineScripts.States
{
    public class BaseState : IState
    {
        protected readonly PlayerStateMachine stateMachine;
        protected readonly PlayerCondition playerCondition;
        protected Coroutine staminaCoroutine;
        protected Coroutine reloadCoroutine;
        protected Coroutine footStepCoroutine;

        private float speed;
        
        public BaseState(PlayerStateMachine machine)
        {
            stateMachine = machine;
            playerCondition = stateMachine.Player.PlayerCondition;
        }
        
        public virtual void Enter()
        {
            AddInputActionCallbacks();
        }

        public virtual void HandleInput()
        {
            if (playerCondition.IsDead) { stateMachine.MovementDirection = Vector2.zero; return; }
            ReadMovementInput();
        }

        public virtual void Update()
        {
            if (playerCondition.IsDead) { return; }
            Move();
        }

        public virtual void LateUpdate()
        {
            Rotate(stateMachine.Player.MainCameraTransform.forward);
        }

        public virtual void PhysicsUpdate()
        {
            
        }

        public virtual void Exit()
        {
            RemoveInputActionCallbacks();
        }
        
        protected void StartAnimation(int animatorHash)
        {
            stateMachine.Player.Animator.SetBool(animatorHash, true);
        }

        protected void StopAnimation(int animatorHash)
        {
            stateMachine.Player.Animator.SetBool(animatorHash, false);
        }
        
        protected virtual void ReadMovementInput()
        {
            stateMachine.MovementDirection =
                stateMachine.Player.PlayerInput.PlayerActions.Move.ReadValue<Vector2>();
        }
        
        private void Move()
        {
            var movementDirection = GetMovementDirection();
            Move(movementDirection);
        }

        private Vector3 GetMovementDirection()
        {
            var forward = stateMachine.MainCameraTransform.forward;
            var right = stateMachine.MainCameraTransform.right;


            forward.y = 0;
            right.y = 0;
            
            forward.Normalize();
            right.Normalize();
            
            return forward * stateMachine.MovementDirection.y + right * stateMachine.MovementDirection.x;
        }

        private void Move(Vector3 direction)
        {
            var targetSpeed = direction == Vector3.zero ? 0f : GetMovementSpeed();
            var currentHorizontalSpeed = new Vector3(stateMachine.Player.Controller.velocity.x, 0f,  stateMachine.Player.Controller.velocity.z).magnitude;
            
            if (!Mathf.Approximately(currentHorizontalSpeed, targetSpeed))
            {
                speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed,
                    Time.deltaTime * 10f);
            }
            else speed = targetSpeed;
            // Service.Log(speed.ToString());
            stateMachine.Player.Animator.SetFloat(stateMachine.Player.AnimationData.SpeedParameterHash, speed);
            stateMachine.Player.Controller.Move(direction * (speed * Time.deltaTime) + stateMachine.Player.PlayerGravity.ExtraMovement * Time.deltaTime);
        }
        
        private float GetMovementSpeed()
        {
            var movementSpeed = stateMachine.MovementSpeed * stateMachine.MovementSpeedModifier;
            return movementSpeed;
        }

        private void Rotate(Vector3 direction)
        {
            if (playerCondition.IsDead) return;
            if (direction == Vector3.zero) return;
            
            var unitTransform = stateMachine.Player.transform;
            var cameraPivotTransform = stateMachine.Player.CameraPivot; 
            
            var unitDirection = new Vector3(direction.x, 0, direction.z);
            var targetRotation = Quaternion.LookRotation(unitDirection);
            unitTransform.rotation = Quaternion.Slerp(unitTransform.rotation, targetRotation, stateMachine.RotationDamping * Time.unscaledDeltaTime);
            
            var cameraTargetRotation = Quaternion.LookRotation(direction);
            cameraPivotTransform.rotation = cameraTargetRotation;
        }

        private void AddInputActionCallbacks()
        {
            var playerInput = stateMachine.Player.PlayerInput;
            playerInput.PlayerActions.Move.canceled += OnMoveCanceled;
            playerInput.PlayerActions.Jump.started += OnJumpStarted;
            playerInput.PlayerActions.Run.started += OnRunStarted;
            playerInput.PlayerActions.Crouch.started += OnCrouchStarted;
            playerInput.PlayerActions.Reload.started += OnReloadStarted;
            playerInput.PlayerActions.Interact.started += OnInteractStarted;
            playerInput.PlayerActions.Aim.started += OnAimStarted;
            playerInput.PlayerActions.Aim.canceled += OnAimCanceled;
            playerInput.PlayerActions.Fire.started += OnFireStarted;
            playerInput.PlayerActions.Fire.canceled += OnFireCanceled;
            playerInput.PlayerActions.SwitchWeapon.performed += OnSwitchByScroll;
            playerInput.PlayerActions.SwitchToMain.started += OnSwitchToMain;
            playerInput.PlayerActions.SwitchToSub.started += OnSwitchToSecondary;
            playerInput.PlayerActions.SwitchToBomb.started += OnSwitchToGrenade;
        }
        
        private void RemoveInputActionCallbacks()
        {
            var playerInput = stateMachine.Player.PlayerInput;
            playerInput.PlayerActions.Move.canceled -= OnMoveCanceled;
            playerInput.PlayerActions.Jump.started -= OnJumpStarted;
            playerInput.PlayerActions.Run.started -= OnRunStarted;
            playerInput.PlayerActions.Crouch.started -= OnCrouchStarted;
            playerInput.PlayerActions.Reload.started -= OnReloadStarted;
            playerInput.PlayerActions.Interact.started -= OnInteractStarted;
            playerInput.PlayerActions.Aim.started -= OnAimStarted;
            playerInput.PlayerActions.Aim.canceled -= OnAimCanceled;
            playerInput.PlayerActions.Fire.started -= OnFireStarted;
            playerInput.PlayerActions.Fire.canceled -= OnFireCanceled;
            playerInput.PlayerActions.SwitchWeapon.performed -= OnSwitchByScroll;
            playerInput.PlayerActions.SwitchToMain.started -= OnSwitchToMain;
            playerInput.PlayerActions.SwitchToSub.started -= OnSwitchToSecondary;
            playerInput.PlayerActions.SwitchToBomb.started -= OnSwitchToGrenade;
        }

        protected IEnumerator RecoverStamina_Coroutine(float recoverRate, float interval)
        {
            while (playerCondition.CurrentStamina < playerCondition.MaxStamina)
            {
                playerCondition.OnRecoverStamina(recoverRate);
                yield return new WaitForSeconds(interval);
            }
        }
        
        protected IEnumerator ConsumeStamina_Coroutine(float consumeRate, float interval)
        {
            while (playerCondition.CurrentStamina > 0)
            {
                playerCondition.OnConsumeStamina(consumeRate);
                yield return new WaitForSeconds(interval);
            }
        }

        /* - 기본동작 관련 메소드 - */
        protected virtual void OnMoveCanceled(InputAction.CallbackContext context) { }
        protected virtual void OnJumpStarted(InputAction.CallbackContext context) { if (playerCondition.IsDead) return; }
        protected virtual void OnRunStarted(InputAction.CallbackContext context) { if (playerCondition.IsDead) return; }
        protected virtual void OnCrouchStarted(InputAction.CallbackContext context) { if (playerCondition.IsDead) return; }
        /* -------------------- */
        
        /* - Aim 관련 메소드 - */
        protected virtual void OnAimStarted(InputAction.CallbackContext context)
        {
            if (playerCondition.IsDead || playerCondition.IsSwitching) return;
            playerCondition.OnAim(true, stateMachine.Player.ZoomFoV, stateMachine.Player.TransitionTime);
        }
        protected virtual void OnAimCanceled(InputAction.CallbackContext context)
        {
            if (playerCondition.IsDead || playerCondition.IsSwitching) return;
            playerCondition.OnAim(false, stateMachine.Player.OriginalFoV, stateMachine.Player.TransitionTime);
        }
        /* ----------------- */
        
        /* - Fire & Reload 관련 메소드 - */
        protected virtual void OnFireStarted(InputAction.CallbackContext context)
        {
            if (playerCondition.IsDead || playerCondition.IsSwitching) return;
            playerCondition.IsAttacking = true;
        }
        protected virtual void OnFireCanceled(InputAction.CallbackContext context)
        {
            playerCondition.IsAttacking = false;
        }
        
        protected virtual void OnReloadStarted(InputAction.CallbackContext context)
        {
            if (playerCondition.IsDead || playerCondition.IsSwitching) return;
        }
        protected IEnumerator Reload_Coroutine(float interval)
        {
            if (playerCondition.Weapons[playerCondition.EquippedWeaponIndex] is Gun gun)
            {
                if (gun.CurrentAmmoCount <= 0 || gun.CurrentAmmoCountInMagazine == gun.MaxAmmoCountInMagazine)
                    yield break;

                // TODO: Play Animation
                gun.IsReloading = true;
                yield return new WaitForSeconds(interval);
                gun.OnReload();
                gun.IsReloading = false;
                
            } else if (playerCondition.Weapons[playerCondition.EquippedWeaponIndex] is GrenadeLauncher grenadeLauncher)
            {
                if (grenadeLauncher.CurrentAmmoCount <= 0 || grenadeLauncher.CurrentAmmoCountInMagazine == grenadeLauncher.MaxAmmoCountInMagazine) 
                    yield break;

                // TODO: Play Animation
                grenadeLauncher.IsReloading = true;
                yield return new WaitForSeconds(interval);
                grenadeLauncher.OnReload();
                grenadeLauncher.IsReloading = false;
            }
            reloadCoroutine = null;
        }
        /* ---------------------------- */
        
        /* - Weapon Switch 관련 메소드 - */
        protected virtual void OnSwitchToMain(InputAction.CallbackContext context)
        {
            if (playerCondition.IsSwitching) return;
            
            int weaponCount = playerCondition.Weapons.Count;
            
            if (weaponCount == 0) return;
            playerCondition.OnSwitchWeapon(0, 0.5f);
        }
        protected virtual void OnSwitchToSecondary(InputAction.CallbackContext context)
        {
            if (playerCondition.IsSwitching) return;
            
            int weaponCount = playerCondition.Weapons.Count;
            
            if (weaponCount == 0) return;
            playerCondition.OnSwitchWeapon(1, 0.5f);
        }

        protected virtual void OnSwitchToGrenade(InputAction.CallbackContext context)
        {
            if (playerCondition.IsSwitching) return;

            int weaponCount = playerCondition.Weapons.Count;

            if (weaponCount == 0) return;
            playerCondition.OnSwitchWeapon(2, 0.5f);
        }
        
        protected virtual void OnSwitchByScroll(InputAction.CallbackContext context)
        {
            if (playerCondition.IsSwitching) return;
            
            var value = context.ReadValue<Vector2>();
            int nextIndex = GetAvailableWeaponIndex(value.y, playerCondition.EquippedWeaponIndex);
            playerCondition.OnSwitchWeapon(nextIndex, 0.5f);
        }

        private int GetAvailableWeaponIndex(float direction, int currentIndex)
        {
            int count = playerCondition.Weapons.Count;
            if (count == 0) return -1;
            
            if (playerCondition.AvailableWeapons.Count <= 0) return -1;

            var dir = direction < 0f ? 1 : direction > 0f ? -1 : 0;
            if (dir == 0) return -1;

            // 양쪽 끝 무기를 현재 들고 있을 때
            if (currentIndex == 0) { if (dir == -1) return -1; }
            if (currentIndex >= count - 1) { if (dir == 1) return -1; }
            int nextIndex = currentIndex < 0 ? (dir == 1 ? -1 : 0) : currentIndex;
            
            for (var i = 0; i < count; i++)
            {
                nextIndex = (nextIndex + dir + count) % count;
                if (playerCondition.AvailableWeapons[nextIndex])
                {
                    if (nextIndex == currentIndex) return -1;
                    return nextIndex;
                }
            }
            return -1;
        }
        /* --------------------------- */
        
        /* - Interact 관련 메소드 - */
        protected virtual void OnInteractStarted(InputAction.CallbackContext context)
        {
            if (playerCondition.IsDead) return;
            if (stateMachine.Player.PlayerInteraction.Interactable == null) return;

            IInteractable interactable = stateMachine.Player.PlayerInteraction.Interactable;
            if (interactable is DummyWeapon gun)
            {
                gun.OnInteract(stateMachine.Player.gameObject);
            }
        }
        /* ---------------------- */
        
        /* - Skill 관련 메소드 - */
        protected virtual void OnFocusStarted(InputAction.CallbackContext context)
        {
            
        }
        protected virtual void OnInstinctStarted(InputAction.CallbackContext context)
        {
            
        }
        /* -------------------- */
    }
}