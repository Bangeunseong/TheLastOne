using System;
using System.Collections;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Interfaces.Player;
using _1.Scripts.Item.Items;
using _1.Scripts.Manager.Core;
using _1.Scripts.Weapon.Scripts.Common;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _1.Scripts.Entity.Scripts.Player.StateMachineScripts.States
{
    public class BaseState : IState
    {
        protected readonly PlayerStateMachine stateMachine;
        protected readonly PlayerCondition playerCondition;
        protected readonly CoreManager coreManager;
        
        protected Coroutine staminaCoroutine;
        protected Coroutine crouchCoroutine;
        
        private float speed;
        private float smoothVelocity = 5f;
        private Vector3 recoilEuler;
        
        public BaseState(PlayerStateMachine machine)
        {
            stateMachine = machine;
            coreManager = CoreManager.Instance;
            playerCondition = stateMachine.Player.PlayerCondition;
        }
        
        public virtual void Enter()
        {
            AddInputActionCallbacks();
        }

        public virtual void HandleInput()
        {
            if (coreManager.gameManager.IsGamePaused) { stateMachine.MovementDirection = Vector2.zero; return; }
            ReadMovementInput();
        }

        public virtual void PhysicsUpdate()
        {
            
        }

        public virtual void Update()
        {
            Move();
        }

        public virtual void LateUpdate()
        {
            var baseForward = stateMachine.Player.MainCameraTransform.forward;
            var baseRot = Quaternion.LookRotation(baseForward);
            var recoilRot = Quaternion.Euler(stateMachine.Player.PlayerRecoil.CurrentRotation);
            
            var rotatedForward = baseRot * recoilRot * Vector3.forward;
            Rotate(rotatedForward);
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
            
            return (forward * stateMachine.MovementDirection.y + right * stateMachine.MovementDirection.x).normalized;
        }

        private void Move(Vector3 direction)
        {
            var targetSpeed = direction == Vector3.zero ? 0f : GetMovementSpeed();
            var currentHorizontalSpeed = new Vector3(stateMachine.Player.Controller.velocity.x, 0f, stateMachine.Player.Controller.velocity.z).magnitude * Time.timeScale;

            speed = speed switch
            {
                >= 0 and < 1 => 1,
                >= 1 => Mathf.Lerp(currentHorizontalSpeed, targetSpeed, smoothVelocity * Time.unscaledDeltaTime),
                _ => throw new ArgumentOutOfRangeException()
            };
            // Service.Log($"Current Horizontal Speed : {currentHorizontalSpeed}\n" + $"Current Speed : {speed}, Target Speed : {targetSpeed}");
            
            // Set Animator Speed Parameter (Only Applied to Activated Animator)
            if (playerCondition.WeaponAnimators[playerCondition.EquippedWeaponIndex].isActiveAndEnabled)
                playerCondition.WeaponAnimators[playerCondition.EquippedWeaponIndex]
                    .SetFloat(stateMachine.Player.AnimationData.SpeedParameterHash, currentHorizontalSpeed);
            stateMachine.Player.Animator.SetFloat(stateMachine.Player.AnimationData.SpeedParameterHash, currentHorizontalSpeed);
            
            stateMachine.Player.Controller.Move(direction * (speed * Time.unscaledDeltaTime) + stateMachine.Player.PlayerGravity.ExtraMovement * Time.unscaledDeltaTime);
        }
        
        private float GetMovementSpeed()
        {
            var movementSpeed = stateMachine.MovementSpeed * stateMachine.MovementSpeedModifier * playerCondition.SkillSpeedMultiplier * playerCondition.ItemSpeedMultiplier;
            return movementSpeed;
        }

        private void Rotate(Vector3 direction)
        {
            if (direction == Vector3.zero) return;
            
            var unitTransform = stateMachine.Player.transform;
            var cameraPivotTransform = stateMachine.Player.CameraPivot; 
            
            var unitDirection = new Vector3(direction.x, 0, direction.z);
            var targetRotation = Quaternion.LookRotation(unitDirection);
            unitTransform.rotation = Quaternion.Slerp(unitTransform.rotation, targetRotation, stateMachine.RotationDamping * Time.unscaledDeltaTime);
            
            var cameraTargetRotation = Quaternion.LookRotation(direction);
            cameraPivotTransform.rotation = cameraTargetRotation;
        }

        protected IEnumerator Crouch_Coroutine(bool isCrouch, float duration)
        {
            var currentPosition = stateMachine.Player.CameraPivot.localPosition;
            var targetPosition = !isCrouch
                ? stateMachine.Player.IdlePivot.localPosition
                : stateMachine.Player.CrouchPivot.localPosition;

            float t = 0;
            while (t < duration)
            {
                if (!coreManager.gameManager.IsGamePaused) t += Time.unscaledDeltaTime;
                float elapsed = t / duration;
                stateMachine.Player.CameraPivot.localPosition = Vector3.Lerp(currentPosition, targetPosition, elapsed);
                yield return null;
            }
            
            stateMachine.Player.CameraPivot.localPosition = targetPosition;
            crouchCoroutine = null;
        }

        protected IEnumerator RecoverStamina_Coroutine(float recoverRate, float interval)
        {
            while (playerCondition.CurrentStamina < playerCondition.MaxStamina)
            {
                if (!coreManager.gameManager.IsGamePaused)
                    playerCondition.OnRecoverStamina(recoverRate);
                yield return new WaitForSecondsRealtime(interval);
            }
        }
        
        protected IEnumerator ConsumeStamina_Coroutine(float consumeRate, float interval)
        {
            while (playerCondition.CurrentStamina > 0)
            {
                if (!coreManager.gameManager.IsGamePaused)
                    playerCondition.OnConsumeStamina(consumeRate);
                yield return new WaitForSecondsRealtime(interval);
            }
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
            playerInput.PlayerActions.Focus.started += OnFocusStarted;
            playerInput.PlayerActions.Instinct.started += OnInstinctStarted;
            playerInput.PlayerActions.ItemAction.started += OnItemActionStarted;
            playerInput.PlayerActions.ItemAction.canceled += OnItemActionCanceled;
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
            playerInput.PlayerActions.Focus.started -= OnFocusStarted;
            playerInput.PlayerActions.Instinct.started -= OnInstinctStarted;
            playerInput.PlayerActions.ItemAction.started -= OnItemActionStarted;
            playerInput.PlayerActions.ItemAction.canceled -= OnItemActionCanceled;
        }
        
        /* - 기본동작 관련 메소드 - */
        protected virtual void OnMoveCanceled(InputAction.CallbackContext context) { }
        protected virtual void OnJumpStarted(InputAction.CallbackContext context) { }
        protected virtual void OnRunStarted(InputAction.CallbackContext context) { }
        protected virtual void OnCrouchStarted(InputAction.CallbackContext context) { }
        /* -------------------- */
        
        /* - Aim 관련 메소드 - */
        protected virtual void OnAimStarted(InputAction.CallbackContext context)
        {
            if (playerCondition.IsSwitching) return;
            playerCondition.OnAim(true, stateMachine.Player.ZoomFoV, stateMachine.Player.TransitionTime);
        }
        protected virtual void OnAimCanceled(InputAction.CallbackContext context)
        {
            if (playerCondition.IsSwitching) return;
            playerCondition.OnAim(false, stateMachine.Player.OriginalFoV, stateMachine.Player.TransitionTime);
        }
        /* ----------------- */
        
        /* - Fire & Reload 관련 메소드 - */
        protected virtual void OnFireStarted(InputAction.CallbackContext context)
        {
            if (playerCondition.IsSwitching) return;
            playerCondition.IsAttacking = true;
        }
        protected virtual void OnFireCanceled(InputAction.CallbackContext context)
        {
            playerCondition.IsAttacking = false;
        }
        protected virtual void OnReloadStarted(InputAction.CallbackContext context)
        {
            
        }
        /* ---------------------------- */
        
        /* - Weapon Switch 관련 메소드 - */
        private void OnSwitchToMain(InputAction.CallbackContext context)
        {
            if (playerCondition.IsSwitching) return;
            
            int weaponCount = playerCondition.Weapons.Count;
            
            if (weaponCount == 0) return;
            playerCondition.OnSwitchWeapon(1, 0.5f);
        }
        private void OnSwitchToSecondary(InputAction.CallbackContext context)
        {
            if (playerCondition.IsSwitching) return;
            
            int weaponCount = playerCondition.Weapons.Count;
            
            if (weaponCount == 0) return;
            playerCondition.OnSwitchWeapon(2, 0.5f);
        }
        private void OnSwitchToGrenade(InputAction.CallbackContext context)
        {
            if (playerCondition.IsSwitching) return;

            int weaponCount = playerCondition.Weapons.Count;

            if (weaponCount == 0) return;
            playerCondition.OnSwitchWeapon(3, 1f);
        }
        private  void OnSwitchByScroll(InputAction.CallbackContext context)
        {
            if (playerCondition.IsSwitching) return;
            
            var value = context.ReadValue<Vector2>();
            int nextIndex = GetAvailableWeaponIndex(value.y, playerCondition.EquippedWeaponIndex);
            playerCondition.OnSwitchWeapon(nextIndex, nextIndex > 1 ? 1f : 0.5f);
        }
        private int GetAvailableWeaponIndex(float direction, int currentIndex)
        {
            int count = playerCondition.Weapons.Count;
            if (count == 0) return 0;
            
            if (playerCondition.AvailableWeapons.Count <= 0) return 0;

            var dir = direction < 0f ? 1 : direction > 0f ? -1 : 0;
            if (dir == 0) return 0;

            int nextIndex = currentIndex;
            for (var i = 0; i < count; i++)
            {
                nextIndex = (nextIndex + dir + count) % count;
                if (playerCondition.AvailableWeapons[nextIndex])
                {
                    return nextIndex;
                }
            }
            return 0;
        }
        /* --------------------------- */
        
        /* - Interact 관련 메소드 - */
        protected virtual void OnInteractStarted(InputAction.CallbackContext context)
        {
            if (playerCondition.IsDead) return;

            IInteractable interactable = stateMachine.Player.PlayerInteraction.Interactable;
            switch (interactable)
            {
                case DummyWeapon gun:
                    gun.OnInteract(stateMachine.Player.gameObject);
                    break;
                case DummyItem item:
                    item.OnInteract(stateMachine.Player.gameObject);
                    break;
            }
        }
        /* ---------------------- */
        
        /* - Skill 관련 메소드 - */
        protected virtual void OnFocusStarted(InputAction.CallbackContext context)
        {
            if (!playerCondition.OnConsumeFocusGauge()) return;
        }
        protected virtual void OnInstinctStarted(InputAction.CallbackContext context)
        {
            if (!playerCondition.OnConsumeInstinctGauge()) return;
        }
        /* -------------------- */
        
        /* - Item 관련 메소드 - */
        protected virtual void OnItemActionStarted(InputAction.CallbackContext context)
        {
            stateMachine.Player.PlayerInventory.OnItemActionStarted();
        }

        protected virtual void OnItemActionCanceled(InputAction.CallbackContext context)
        {
            stateMachine.Player.PlayerInventory.OnItemActionCanceled();
        }
        /* ------------------- */
    }
}