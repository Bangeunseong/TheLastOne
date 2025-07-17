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
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button quitButton;
        private PauseHandler pauseHandler;

        
        
        public override void Show() { panel.SetActive(true); }
        public override void Hide() { panel.SetActive(false); }
        public override void ResetUI() { Hide(); }

        private void Start()
        {
            pauseHandler = FindObjectOfType<PauseHandler>();
            resumeButton.onClick.AddListener(() => pauseHandler.ClosePausePanel());
            quitButton.onClick.AddListener(() => { CoreManager.Instance.MoveToIntroScene(); });
            Hide();
        }
    }
}