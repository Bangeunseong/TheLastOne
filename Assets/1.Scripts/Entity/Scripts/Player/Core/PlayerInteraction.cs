using _1.Scripts.Interfaces;
using _1.Scripts.Interfaces.Player;
using Unity.Collections;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.Player.Core
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float checkRate = 0.05f;
        [SerializeField] private float maxCheckDistance = 5f;
        [SerializeField] private LayerMask interactableLayers;
        [SerializeField] private GameObject detectedObject;

        // Fields
        [Header("Last Check Time")]
        [SerializeField, ReadOnly] private float timeSinceLastCheck;
        private Camera cam;
        
        // Properties
        public IInteractable Interactable { get; private set; }
        
        // Start is called before the first frame update
        private void Start()
        {
            cam = Camera.main;
            timeSinceLastCheck = 0;
        }

        // Update is called once per frame
        private void Update()
        {
            if (timeSinceLastCheck < checkRate) { timeSinceLastCheck += Time.unscaledDeltaTime; return; }

            timeSinceLastCheck = 0;
            var ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            if (Physics.Raycast(ray, out var hit, maxCheckDistance, interactableLayers))
            {
                if (hit.collider.gameObject == detectedObject) return;
                
                detectedObject = hit.collider.gameObject;
                Interactable = detectedObject.TryGetComponent<IInteractable>(out var interactable) ? interactable : null;
            }
            else
            {
                detectedObject = null;
                Interactable = null;
            }
        }

        public void OnInteract()
        {
            Interactable.OnInteract(gameObject);
            ResetParameters();
        }

        public void ResetParameters()
        {
            detectedObject = null;
            Interactable = null;
        }
    }
}