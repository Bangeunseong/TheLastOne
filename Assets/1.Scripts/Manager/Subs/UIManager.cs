using System;
using System.Collections.Generic;
using _1.Scripts.Manager.Core;
using _1.Scripts.UI;
using _1.Scripts.UI.InGame;
using _1.Scripts.UI.InGame.Mission;
using _1.Scripts.UI.Loading;
using _1.Scripts.UI.Lobby;
using _1.Scripts.UI.Setting;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace _1.Scripts.Manager.Subs
{
    public enum CurrentState
    {
        Lobby,
        Loading,
        InGame,
        None
    }
    
    [Serializable] public class UIManager
    {
        [field: Header("UI Components")]
        [field: SerializeField] public SerializedDictionary<CurrentState, List<UIBase>> LoadedUI { get; private set; } = new();
        
        [field: Header("InGameUI")]
        [field: SerializeField] public InGameUI InGameUI { get; private set; }

        [field: Header("MinigameUI")]
        [field: SerializeField] public MinigameUI MinigameUI { get; private set; }

        private Transform uiRoot;
        private CurrentState currentState = CurrentState.None;
        private LobbyUI lobbyUI;
        private LoadingUI loadingUI;
        private SettingUI settingUI;
        private MissionUI missionUI;
        private DistanceUI distanceUI;
        private CoreManager coreManager;

        public LoadingUI LoadingUI => loadingUI;
        
        private const string INGAME_UI_ADDRESS = "InGameUI";
        private const string MINIGAME_UI_ADDRESS = "MiniGameUI";
        
        public void Start()
        {
            coreManager = CoreManager.Instance;
            
            var mainCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
            if (mainCanvas) { uiRoot = mainCanvas.transform; }
            
            lobbyUI = GameObject.Find("LobbyUI")?.GetComponent<LobbyUI>();
            loadingUI = GameObject.Find("LoadingUI")?.GetComponent<LoadingUI>();
            
            lobbyUI?.Init(this);
            loadingUI?.Init(this);
            
            lobbyUI?.SetActive(false);
            loadingUI?.SetActive(false);
            
            settingUI?.Initialize();
            
            ChangeState(CurrentState.Lobby);
        }
        
        public void ChangeState(CurrentState newState)
        {
            if (currentState == newState) return;
            
            DeactivateState(currentState);
            currentState = newState;
            ActivateState(currentState);
        }

        private void ActivateState(CurrentState state)
        {
            switch (state)
            {
                case CurrentState.Lobby:
                    lobbyUI?.SetActive(true);
                    break;
                case CurrentState.Loading:
                    loadingUI?.SetActive(true);
                    break;
                case CurrentState.InGame:
                    InGameUI = LoadUI<InGameUI>(state, INGAME_UI_ADDRESS);
                    MinigameUI = LoadUI<MinigameUI>(state, MINIGAME_UI_ADDRESS);
                    break;
            }
        }

        private void DeactivateState(CurrentState state)
        {
            switch (state)
            {
                case CurrentState.Lobby: lobbyUI?.SetActive(false); break;
                case CurrentState.Loading: loadingUI?.SetActive(false); break;
                case CurrentState.InGame:
                    if (LoadedUI.TryGetValue(state, out var list)) { foreach (var ui in list) ui.SetActive(false); }
                    InGameUI?.ResetUI();
                    break;
                case CurrentState.None:
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
        
        private T LoadUI<T>(CurrentState state, string address) where T : UIBase
        {
            if (LoadedUI.TryGetValue(state, out var list))
            {
                foreach (var existing in list)
                    if (existing is T found)
                    {
                        Service.Log($"{found.name}"); 
                        found.Init(this); found.SetActive(true);
                        return found;
                    }
            }
            
            var prefab = coreManager.resourceManager.GetAsset<GameObject>(address);
            if (prefab == null || uiRoot == null) return null;
            
            var instance  = Object.Instantiate(prefab, uiRoot, false);
            
            if (!instance.TryGetComponent(out T component)) return null;
            if (component is InGameUI inGameUI) inGameUI.Init(this);
            else if(component is MinigameUI minigameUI) minigameUI.Init(this);
            
            if (LoadedUI.ContainsKey(state)) { LoadedUI[state].Add(component); return component; }
            LoadedUI[state] = new List<UIBase> { component };
            return component;
        }
        public MinigameUI ShowMinigameUI()
        {
            var ui = LoadUI<MinigameUI>(CurrentState.InGame, MINIGAME_UI_ADDRESS);
            ui.SetActive(true);
            return ui;
        }

        public void HideMinigameUI()
        {
            if (!LoadedUI.TryGetValue(CurrentState.InGame, out var list)) return;
            
            foreach (var baseUi in list)
            {
                if (baseUi is MinigameUI minigameUI)
                    minigameUI.HidePanel();
            }
        }

        public void HideInGameUI()
        {
            if (!LoadedUI.TryGetValue(CurrentState.InGame, out var list)) return;
            
            foreach (var ui in list)
                ui.SetActive(false);
        }

        public void ShowInGameUI()
        {
            if (!LoadedUI.TryGetValue(CurrentState.InGame, out var list)) return;
            
            foreach (var ui in list)
                ui.SetActive(true);
        }

        public void OnCutsceneStarted(PlayableDirector director)
        {
            HideInGameUI();
        }
        
        public void OnCutsceneStopped(PlayableDirector director)
        {
            ShowInGameUI();
            
            director.played -= OnCutsceneStarted;
            director.stopped -= OnCutsceneStopped;
        }
    }
}