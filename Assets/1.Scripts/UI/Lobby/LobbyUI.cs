using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using UnityEngine;
using UnityEngine.UI;

namespace _1.Scripts.UI.Lobby
{
    public class LobbyUI : UIBase
    {
        [Header("Lobby UI")] 
        [SerializeField] private Button startButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button settingButton;
        [SerializeField] private Button exitButton;
        
        public override void Init(UIManager manager)
        {
            base.Init(manager);
            
            startButton.onClick.AddListener(OnStartButtonClicked);
            loadButton.onClick.AddListener(OnLoadButtonClicked);
            exitButton.onClick.AddListener(OnQuitButtonClicked);
        }
        
        public override void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        private void OnStartButtonClicked()
        {
            Debug.Log("Start Button Clicked");
            CoreManager.Instance.StartGame();
        }
        
        private void OnLoadButtonClicked()
        {
            CoreManager.Instance.StartGame();
        }

        private void OnQuitButtonClicked()
        {
            Application.Quit();
        }
    }
}