using _1.Scripts.Manager.Core;
using _1.Scripts.UI.Common;
using Michsky.UI.Shift;
using UnityEngine;

namespace _1.Scripts.UI
{
    public class PauseHandler : MonoBehaviour
    {
        [SerializeField] private BlurManager blurMgr;
        [SerializeField] private Animator pauseAnimator;
        [SerializeField] private InventoryHandler inventoryHandler;
        
        [Header("Setting Panel")]
        [SerializeField] private CanvasGroup settingPanel;
        [SerializeField] private Animator settingAnimator;
        
        private PauseMenuUI pauseMenuUI;
        private CoreManager coreManager;

        private bool isPaused;
        public bool IsPaused => isPaused;
        public void SetInventoryHandler(InventoryHandler handler) => inventoryHandler = handler;
        
        public void Initialize(PauseMenuUI ui)
        {
            pauseMenuUI = ui;
        }

        public void TogglePause()
        {
            if (inventoryHandler && inventoryHandler.IsInventoryOpen)
            {
                inventoryHandler.ToggleInventory();
                return;
            }
            isPaused = !isPaused;
            if (isPaused) Pause();
            else Resume();
        }

        public void ClosePausePanel()
        {
            Resume();
        }

        private void Pause()
        {
            if (!pauseMenuUI) return;
            CoreManager.Instance.uiManager.HideHUD();
            CoreManager.Instance.gameManager.PauseGame();
            blurMgr.BlurInAnim();
            pauseMenuUI.Show();
            pauseAnimator.Play("Window In");

            if (settingPanel && settingPanel.alpha > 0f)
            {
                settingAnimator?.Play("Panel Out");
                settingPanel.alpha = 0f;
                settingPanel.interactable = false;
                settingPanel.blocksRaycasts = false;
            }
        }

        private void Resume()
        {
            if (!pauseMenuUI) return;
            CoreManager.Instance.uiManager.ShowHUD();
            CoreManager.Instance.gameManager.ResumeGame();
            blurMgr.BlurOutAnim();
            pauseAnimator.Play("Window Out");
            pauseMenuUI.Hide();
            
            if (settingPanel && settingPanel.alpha > 0f)
            {
                settingAnimator?.Play("Panel Out");
                settingPanel.alpha = 0f;
                settingPanel.interactable = false;
                settingPanel.blocksRaycasts = false;
            }
        }

        public void SetPauseMenuUI(PauseMenuUI ui)
        {
            pauseMenuUI = ui;
            settingPanel = ui.SettingPanel;
            settingAnimator = ui.SettingAnimator;
        }
    }
}