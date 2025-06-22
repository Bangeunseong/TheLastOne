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
            var movementSpeed = GetMovementSpeed();
            stateMachine.Player.Controller.Move((direction * movementSpeed + stateMachine.Player.PlayerGravity.ExtraMovement) * Time.unscaledDeltaTime);
        }
        
        private float GetMovementSpeed()
        {
            var movementSpeed = stateMachine.MovementSpeed;
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
        }
        
        private void RemoveInputActionCallbacks()
        {
            var playerInput = stateMachine.Player.PlayerInput;
            playerInput.PlayerActions.Move.canceled -= OnMoveCanceled;
            playerInput.PlayerActions.Jump.started -= OnJumpStarted;
            playerInput.PlayerActions.Run.started -= OnRunStarted;
            playerInput.PlayerActions.Crouch.started -= OnCrouchStarted;
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
    }
}