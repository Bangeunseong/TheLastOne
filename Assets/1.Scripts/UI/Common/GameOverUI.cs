using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Manager.Core;
using UnityEngine;
using UnityEngine.UI;

namespace _1.Scripts.UI.Common
{
    public class GameOverUI : UIBase
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button reloadButton;
        [SerializeField] private Button quitButton;
        private PlayerCondition playerCondition;
        public override void Show() { panel.SetActive(true); }
        public override void Hide() { panel.SetActive(false); }

        public override void Initialize(object param = null)
        {
            if (playerCondition) playerCondition.OnDeath -= OnPlayerDeath;
            if (param is PlayerCondition newPlayerCondition)
            {
                playerCondition = newPlayerCondition;
                playerCondition.OnDeath += OnPlayerDeath;
            }
        }

        private void Awake()
        {
            reloadButton.onClick.AddListener(() => { Hide(); CoreManager.Instance.ReloadGame(); });
            quitButton.onClick.AddListener(() => { Hide(); CoreManager.Instance.MoveToIntroScene(); });
            Hide();
        }
        private void OnDestroy()
        {
            if (playerCondition) playerCondition.OnDeath -= OnPlayerDeath;
        }

        private void OnPlayerDeath()
        {
            Show();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}