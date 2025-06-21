using UnityEngine;

namespace _1.Scripts.Entity.Scripts.Player.Core
{
    public class PlayerGravity : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private CharacterController characterController;
        
        private float verticalVelocity;
        
        public Vector3 ExtraMovement => Vector3.up * verticalVelocity;

        private void Awake()
        {
            if(!characterController) characterController = this.TryGetComponent<CharacterController>();
        }

        private void Reset()
        {
            if(!characterController) characterController = this.TryGetComponent<CharacterController>();
        }

        private void Update()
        {
            if (characterController.isGrounded && verticalVelocity < 0f) verticalVelocity = Physics.gravity.y * Time.unscaledDeltaTime;
            else verticalVelocity += Physics.gravity.y * Time.unscaledDeltaTime;
        }
        
        public void Jump(float jumpForce)
        {
            verticalVelocity = jumpForce;
        }
    }
}