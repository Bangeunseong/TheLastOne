using System.Collections;
using System.Globalization;
using _1.Scripts.Entity.Scripts.Common;
using _1.Scripts.Entity.Scripts.Player.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _1.Scripts.Entity.Scripts.Player.StateMachineScripts.States
{
    public class BaseState : IState
    {
        protected readonly PlayerStateMachine stateMachine;
        protected readonly PlayerCondition playerCondition;
        protected Coroutine staminaCoroutine;

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
        }
        
        private void RemoveInputActionCallbacks()
        {
            var playerInput = stateMachine.Player.PlayerInput;
            playerInput.PlayerActions.Move.canceled -= OnMoveCanceled;
            playerInput.PlayerActions.Jump.started -= OnJumpStarted;
            playerInput.PlayerActions.Run.started -= OnRunStarted;
            playerInput.PlayerActions.Crouch.started -= OnCrouchStarted;
            playerInput.PlayerActions.Reload.started -= OnReloadStarted;
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

        protected virtual void OnMoveCanceled(InputAction.CallbackContext context) { }
        protected virtual void OnJumpStarted(InputAction.CallbackContext context)
        {
            if (playerCondition.IsDead) return;
        }
        protected virtual void OnRunStarted(InputAction.CallbackContext context)
        {
            if (playerCondition.IsDead) return;
        }
        protected virtual void OnCrouchStarted(InputAction.CallbackContext context)
        {
            if (playerCondition.IsDead) return;
        }
        protected virtual void OnReloadStarted(InputAction.CallbackContext context)
        {
            if (playerCondition.IsDead) return;
        }
    }
}