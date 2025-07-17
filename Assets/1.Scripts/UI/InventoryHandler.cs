using _1.Scripts.Manager.Core;
using _1.Scripts.UI.Inventory;
using Michsky.UI.Shift;
using UnityEngine;

namespace _1.Scripts.UI
{
    public class InventoryHandler : MonoBehaviour
    {
        [Header("Inventory")]
        [SerializeField] private Animator inventoryAnimator;
        [SerializeField] private BlurManager blurMgr;
        [SerializeField] private PauseHandler pauseHandler;
        private InventoryUI inventoryUI;
        private CoreManager coreManager;
        bool isPaused;
        public bool IsInventoryOpen => isPaused;
        
        private void Start()
        {
            coreManager = CoreManager.Instance;
            inventoryUI = coreManager.uiManager.GetUI<InventoryUI>();
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
            blurMgr.BlurInAnim();
            inventoryUI.Show();
            inventoryAnimator?.Play("Panel In");
        }

        private void Resume()
        {
            coreManager.gameManager.ResumeGame();
            blurMgr?.BlurOutAnim();
            inventoryAnimator?.Play("Panel Out");
            inventoryUI.Hide();
        }

        public void CloseInventoryPanel()
        {
            isPaused = false;
            inventoryUI?.Hide();
        }
    }
}