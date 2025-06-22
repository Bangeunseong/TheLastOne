using UnityEngine;

namespace _1.Scripts.Entity.Scripts.Player.Core
{
    public class PlayerInput : MonoBehaviour
    {
        public InputActions PlayerInputs { get; private set; }
        public InputActions.PlayerActions PlayerActions { get; private set; }

        private void Awake()
        {
            PlayerInputs = new InputActions();
            PlayerActions = PlayerInputs.Player;
        }

        private void OnEnable()
        {
            PlayerInputs.Enable();
        }

        private void OnDisable()
        {
            PlayerInputs.Disable();
        }

        private void OnDestroy()
        {
            PlayerInputs.Dispose();
        }
    }
}
