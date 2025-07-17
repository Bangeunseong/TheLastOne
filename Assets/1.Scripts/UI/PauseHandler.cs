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
        private PauseMenuUI pauseMenuUI;
        private CoreManager coreManager;
        private bool isPaused;
        public bool IsPaused => isPaused;

        private void Start()
        {
            coreManager = CoreManager.Instance;
            pauseMenuUI = coreManager.uiManager.GetUI<PauseMenuUI>();
        }

        public void TogglePause()
        {
            if (inventoryHandler != null && inventoryHandler.IsInventoryOpen)
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
            coreManager.gameManager.PauseGame();
            blurMgr.BlurInAnim();
            pauseMenuUI.Show();
            pauseAnimator.Play("Window In");
        }

        private void Resume()
        {
            coreManager.gameManager.ResumeGame();
            blurMgr.BlurOutAnim();
            pauseAnimator.Play("Window Out");
            pauseMenuUI.Hide();
        }
    }
}