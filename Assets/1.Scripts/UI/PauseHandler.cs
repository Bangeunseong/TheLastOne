using _1.Scripts.Manager.Core;
using Michsky.UI.Shift;
using UnityEngine;

namespace _1.Scripts.UI
{
    public class PauseHandler : MonoBehaviour
    {
        [Header("Pause Menu")]
        [SerializeField] private GameObject pauseCanvas;
        [SerializeField] private Animator pauseAnimator;
        [SerializeField] private BlurManager blurMgr;

        [Header("Settings Panel")]
        [SerializeField] private CanvasGroup settingsCanvas;
        [SerializeField] private Animator settingsAnimator;
        
        [SerializeField] private InventoryHandler inventoryHandler;
        
        bool isPaused;
        private CoreManager coreManager;
        
        public bool IsPaused => isPaused;

        private void Start()
        {
            coreManager = CoreManager.Instance;
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
            isPaused = false;
            pauseCanvas?.SetActive(false);
        }

        private void Pause()
        {
            coreManager.gameManager.PauseGame();
            blurMgr.BlurInAnim();
            pauseCanvas.SetActive(true);
            pauseAnimator.Play("Window In");

            if (settingsCanvas.alpha > 0f)
            {
                settingsAnimator.Play("Panel Out");
                settingsCanvas.alpha = 0f;
                settingsCanvas.interactable = false;
                settingsCanvas.blocksRaycasts = false;
            }
        }

        private void Resume()
        {
            coreManager.gameManager.ResumeGame();
            blurMgr.BlurOutAnim();
            if (settingsCanvas.alpha > 0f)
            {
                settingsAnimator.Play("Panel Out");
                settingsCanvas.alpha = 0f;
                settingsCanvas.interactable = false;
                settingsCanvas.blocksRaycasts = false;
            }
            
            pauseAnimator.Play("Window Out");
            
            pauseCanvas.SetActive(false);
        }
    }
}