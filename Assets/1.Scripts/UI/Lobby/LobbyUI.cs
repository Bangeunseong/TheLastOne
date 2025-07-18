using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.UI.Loading;
using UnityEngine;
using UnityEngine.UI;

namespace _1.Scripts.UI.Lobby
{
    public class LobbyUI : UIBase
    {
        [Header("Lobby UI")] 
        [SerializeField] private GameObject panel;
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

        public override void Show()
        {
            Service.Log("Lobby UI Show");
            panel.SetActive(true);
        }
        
        public override void Hide()
        {
            Service.Log("Lobby UI Hide");
            panel.SetActive(false);
        }

        private void OnStartButtonClicked()
        {
            Debug.Log("Start Button Clicked");
            CoreManager.Instance.StartGame();
            Hide();
        }
        
        private void OnLoadButtonClicked()
        {
            CoreManager.Instance.ReloadGame();
        }

        private void OnQuitButtonClicked()
        {
            Application.Quit();
        }
    }
}