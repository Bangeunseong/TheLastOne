using UnityEngine;

namespace _1.Scripts.Entity.Scripts.Player.Core
{
    public class PlayerGravity : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private CharacterController characterController;

        [field: Header("Gravity Settings")] 
        [field: SerializeField] public float Gravity = -9.81f;
        
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
            if (characterController.isGrounded && verticalVelocity < 0f) verticalVelocity = Gravity * Time.unscaledDeltaTime;
            else verticalVelocity += Gravity * Time.unscaledDeltaTime;
        }
        
        public void Jump(float jumpForce)
        {
            verticalVelocity = Mathf.Sqrt(jumpForce * -2f * Gravity);
        }
    }
}