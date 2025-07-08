using UnityEngine;
using UnityEngine.Events;

namespace Michsky.UI.Shift
{
    public class HoldKeyEvent : MonoBehaviour
    {
        [Header("Key")]
        [SerializeField]
        public KeyCode hotkey;

        [Header("Action")]
        [SerializeField]
        public UnityEvent holdAction;
        [SerializeField]
        public UnityEvent releaseAction;

        bool wasHolding = false;

        void Update()
        {
            bool nowHolding = Input.GetKey(hotkey);

            if (nowHolding && !wasHolding)
                holdAction.Invoke();

            if (!nowHolding && wasHolding)
                releaseAction.Invoke();

            wasHolding = nowHolding;
        }
    }
}