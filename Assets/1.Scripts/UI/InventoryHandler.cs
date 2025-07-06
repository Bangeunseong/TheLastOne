using System;
using _1.Scripts.Manager.Core;
using Cinemachine;
using Michsky.UI.Shift;
using UnityEngine;

namespace _1.Scripts.UI
{
    public class InventoryHandler : MonoBehaviour
    {
        [Header("Inventory")]
        [SerializeField] private GameObject inventoryCanvas;
        [SerializeField] private Animator pauseAnimator;
        [SerializeField] private BlurManager blurMgr;
        [SerializeField] private GameObject inventoryPanel;
        private CinemachineVirtualCamera inventoryCamera;
        
        bool isPaused;
        private CoreManager coreManager;
        private PauseHandler pauseHandler;
        public bool IsInventoryOpen => isPaused;
        
        private void Start()
        {
            coreManager = CoreManager.Instance;
            pauseHandler = FindObjectOfType<PauseHandler>();
        }

        public void ToggleInventory()
        {
            if (!isPaused && pauseHandler != null && pauseHandler.IsPaused)
                return;
            isPaused = !isPaused;
            if (isPaused) Pause();
            else Resume();
        }

        private void Pause()
        {
            coreManager.gameManager.PauseGame();
            Time.timeScale = 0f;
            blurMgr.BlurInAnim();
            inventoryCanvas.SetActive(true);
            inventoryPanel.SetActive(true);
            pauseAnimator.Play("Panel In");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            inventoryCamera.Priority = 100;
            inventoryCamera.gameObject.SetActive(true);
        }

        private void Resume()
        {
            coreManager.gameManager.ResumeGame();
            Time.timeScale = 1f;
            blurMgr.BlurOutAnim();

            pauseAnimator.Play("Panel Out");
            
            inventoryCanvas.SetActive(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            inventoryCamera.Priority = 0;
            inventoryCamera.gameObject.SetActive(false);
        }
    }
}