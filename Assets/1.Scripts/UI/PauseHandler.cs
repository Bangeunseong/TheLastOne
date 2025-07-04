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
        
        bool isPaused;
        private CoreManager coreManager;

        private void Start()
        {
            coreManager = CoreManager.Instance;
        }

        public void TogglePause()
        {
            isPaused = !isPaused;
            if (isPaused) Pause();
            else Resume();
        }

        private void Pause()
        {
            coreManager.gameManager.PauseGame();
            Time.timeScale = 0f;
            blurMgr.BlurInAnim();
            pauseCanvas.SetActive(true);
            pauseAnimator.Play("Window In");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

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
            Time.timeScale = 1f;
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

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}