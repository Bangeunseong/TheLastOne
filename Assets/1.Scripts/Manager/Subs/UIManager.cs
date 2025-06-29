using System;
using System.Collections.Generic;
using _1.Scripts.Manager.Core;
using _1.Scripts.UI;
using _1.Scripts.UI.InGame;
using _1.Scripts.UI.Loading;
using _1.Scripts.UI.Lobby;
using _1.Scripts.UI.Setting;
using UnityEngine;

namespace _1.Scripts.Manager.Subs
{
    public enum CurrentState
    {
        Lobby,
        Loading,
        InGame,
        None
    }
    
    [Serializable]
    public class UIManager
    {
        private CurrentState currentState = CurrentState.None;
        
        private LobbyUI lobbyUI;
        private LoadingUI loadingUI;
        private SettingUI settingUI;
        
        private Dictionary<CurrentState, UIBase> loadedUI = new Dictionary<CurrentState, UIBase>();
        private Dictionary<string, UIPopup> loadedPopup = new Dictionary<string, UIPopup>();
        private Stack<UIPopup> popupStack = new Stack<UIPopup>();

        private Transform uiRoot;
        private Transform popupRoot;
        
        public LoadingUI LoadingUI => loadingUI;
        
        private const string INGAME_UI_ADDRESS = "InGameUI";
        private const string INVENTORY_UI_ADDRESS = "InventoryUI";
        
        public void Start()
        {
            var mainCanvas = GameObject.Find("MainCanvas");
            if (mainCanvas != null)
            {
                uiRoot = mainCanvas.transform;
            }
            
            var popupCanvas = GameObject.Find("PopupCanvas");
            if (popupCanvas != null)
            {
                popupRoot = popupCanvas.transform;
            }
            
            lobbyUI = FindUIComponent<LobbyUI>("LobbyUI");
            loadingUI = FindUIComponent<LoadingUI>("LoadingUI");
            settingUI = FindUIComponent<SettingUI>("SettingUI");
            
            lobbyUI?.Init(this);
            loadingUI?.Init(this);
            settingUI?.Init(this);
            
            lobbyUI?.SetActive(false);
            loadingUI?.SetActive(false);
            settingUI?.SetActive(false);
            
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
                    LoadUI<InGameUI>(state, INGAME_UI_ADDRESS);
                    break;
            }
        }

        private void DeactivateState(CurrentState state)
        {
            switch (state)
            {
                case CurrentState.Lobby:
                    lobbyUI?.SetActive(false);
                    break;
                case CurrentState.Loading:
                    loadingUI?.SetActive(false);
                    break;
                
                case CurrentState.InGame:
                    if (loadedUI.TryGetValue(state, out var ui))
                    {
                        ui.SetActive(false);
                    }
                    break;
            }
        }
        
        private void LoadUI<T>(CurrentState state, string address) where T : UIBase
        {
            if (loadedUI.TryGetValue(state, out var ui))
            {
                ui.SetActive(true);
                return;
            }

            var prefab = CoreManager.Instance.resourceManager.GetAsset<GameObject>(address);
            if (prefab != null && uiRoot != null)
            {
                var instance = GameObject.Instantiate(prefab, uiRoot, false);
                var component = instance.GetComponent<T>();
                if (component != null)
                {
                    component.Init(this);
                    loadedUI[state] = component;
                    component.SetActive(true);;
                }
            }
        }
        
        public void ShowSettingPopup()
        {
            if (settingUI == null) return;
            
            settingUI.transform.SetParent(popupRoot, false);
            settingUI.transform.SetAsLastSibling();
            settingUI.SetActive(true);
            popupStack.Push(settingUI);
        }
        
        public T ShowPopup<T>(string address) where T : UIPopup
        {
            if (loadedPopup.TryGetValue(address, out UIPopup cachedPopup))
            {
                cachedPopup.transform.SetParent(popupRoot, false);
                cachedPopup.transform.SetAsLastSibling();
                cachedPopup.SetActive(true);
                popupStack.Push(cachedPopup);
                return cachedPopup as T;
            }

            var prefab = CoreManager.Instance.resourceManager.GetAsset<GameObject>(address);
            if (prefab != null && popupRoot != null)
            {
                var instance = GameObject.Instantiate(prefab, popupRoot, false);
                var component = instance.GetComponent<T>();
                if (component != null)
                {
                    component.Init(this);
                    loadedPopup[address] = component;
                    popupStack.Push(component);
                    component.SetActive(true);
                    return component;
                }
            }
            return null;
        }
        
        public void ClosePopup()
        {
            if (popupStack.Count > 0)
            {
                popupStack.Pop().SetActive(false);
            }
        }
        
        private T FindUIComponent<T>(string name) where T : Component
        {
            foreach (var canvas in Resources.FindObjectsOfTypeAll<Canvas>())
            {
                if (canvas.isRootCanvas)
                {
                    var component = canvas.GetComponentInChildren<T>(true);
                    if (component != null && component.gameObject.name == name)
                    {
                        return component;
                    }
                }
            }
            Debug.LogWarning($"이름이 '{name}'인 UI 오브젝트를 씬에서 찾을 수 없습니다.");
            return null;
        }
    }
}