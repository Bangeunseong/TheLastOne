using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Manager.Core;
using UnityEngine;
using UnityEngine.UI;

namespace _1.Scripts.UI.Common
{
    public class PauseMenuUI : UIBase
    {
        [Header("PauseMenu Elements")]
        [SerializeField] private GameObject panel;
        [SerializeField] private CanvasGroup settingPanel;
        [SerializeField] private Animator settingAnimator;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button reloadButton;
        [SerializeField] private Button quitButton;
        private PauseHandler pauseHandler;

        public CanvasGroup SettingPanel => settingPanel;
        public Animator SettingAnimator => settingAnimator;
        
        public override void Show() {  panel.SetActive(true); }

        public override void Hide()
        {
            panel.SetActive(false);
        }
        public override void ResetUI() { Hide(); }

        private void Awake()
        {
            pauseHandler = FindObjectOfType<PauseHandler>();
            resumeButton.onClick.AddListener(() =>
            {
                pauseHandler.TogglePause();
                pauseHandler.ClosePausePanel();
            });
            reloadButton.onClick.AddListener(() =>
            {
                pauseHandler.TogglePause();
                CoreManager.Instance.ReloadGame();
            });
            quitButton.onClick.AddListener(() => 
            { 
                pauseHandler.TogglePause();
                CoreManager.Instance.MoveToIntroScene(); 
            });
            Hide();
        }
    }
}