using _1.Scripts.Interfaces.Player;
using _1.Scripts.Manager.Core;
using _1.Scripts.UI.InGame;
using Unity.Collections;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.Player.Core
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float checkRate = 0.05f;
        [SerializeField] private float maxCheckDistance = 2.5f;
        [SerializeField] private LayerMask interactableLayers;
        [SerializeField] private GameObject detectedObject;

        [Header("Hover Prompt (World Space UI)")]
        [SerializeField] private Vector3 promptOffset = new Vector3(0, 2.0f, 0);
        
        private InteractionUI interactionUI;
        private GameObject uiInstance;
        private bool isUIAppearing;
        
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
            
            uiInstance = CoreManager.Instance.objectPoolManager.Get("InteractionUI");
            interactionUI = uiInstance.GetComponent<InteractionUI>();
            uiInstance.transform.SetParent(null, false);
            uiInstance.SetActive(false);
            isUIAppearing = false;
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
                HandleUI();
            }
            else
            {
                detectedObject = null;
                Interactable = null;
                HandleUI();
            }

            if (isUIAppearing)
            {
                uiInstance.transform.LookAt(cam.transform.position);
                uiInstance.transform.Rotate(0f, 180f, 0f);
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

        private void HandleUI()
        {
            if (Interactable != null && !isUIAppearing)
            {
                uiInstance.transform.position = detectedObject.transform.position + promptOffset;
                interactionUI.Show();
                isUIAppearing = true;
            }
            else if (Interactable == null && isUIAppearing)
            {
                interactionUI.Hide();
                isUIAppearing = false;
            }
        }
    }
}