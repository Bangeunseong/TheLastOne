using System;
using System.Collections.Generic;
using _1.Scripts.Manager.Core;
using System.Linq;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using _1.Scripts.UI;

namespace _1.Scripts.Manager.Subs
{
    public enum CurrentState
    {
        Lobby,
        Loading,
        InGame,
        Setting,
        Inventory,
        Hacking,
        WeaponTuning,
        Menu,
        None
    }
    [Serializable] public class UIManager
    {
        [Header("Core")]
        [SerializeField] private Managers coreManager;
        
        private CurrentState currentState;
        private CurrentState previousState;

        private Transform _mainCanvas;
        private Transform _popupCanvas;
        private Stack<UIPopup> popupStack = new Stack<UIPopup>();
        private Dictionary<string, UIPopup> popupCache = new Dictionary<string, UIPopup>();
        
        private Dictionary<CurrentState, UIBase> loadedUI = new Dictionary<CurrentState, UIBase>();
        private Dictionary<CurrentState, AsyncOperationHandle<GameObject>> loadingOperation = new Dictionary<CurrentState, AsyncOperationHandle<GameObject>>();
        
        // UI Address 이름
        private const string LOBBY_UI_ADDRESS = "LobbyUI";
        private const string LOADING_UI_ADDRESS = "LoadingUI";
        private const string INGAME_UI_ADDRESS = "InGameUI";
        private const string SETTING_UI_ADDRESS = "SettingUI";
        private const string INVENTORY_UI_ADDRESS = "InventoryUI";
        private const string HACKING_UI_ADDRESS = "HackingUI";
        private const string WEAPONTUNING_UI_ADDRESS = "WeaponTuningUI";
        private const string MENU_UI_ADDRESS = "MenuUI";
        
        /*public LobbyUI LobbyUI => loadedUI[CurrentState.Lobby];
        public LoadingUI LoadingUI => loadedUI[CurrentState.Loading];
        public InGameUI InGameUI => loadedUI[CurrentState.InGame];
        public SettingUI SettingUI => loadedUI[CurrentState.Setting];
        public MenuUI MenuUI => loadedUI[CurrentState.Menu];
        public InventoryUI InventoryUI => loadedUI[CurrentState.Inventory];
        public HackingUI HackingUI => loadedUI[CurrentState.Hacking];
        public WeaponTuningUI WeaponTuningUI => loadedUI[CurrentState.WeaponTuning];*/
        
        // Constructor
        public UIManager(Managers core){ coreManager = core; }
        

        public void Initialize()
        {
            // Managers에서 초기화
            GameObject mainCanvas = GameObject.Find("MainCanvas");
            GameObject popupCanvas = GameObject.Find("PopupCanvas");
            
            if (mainCanvas != null)
            {
                _mainCanvas = mainCanvas.transform;
            }
            else
            {
                Service.Log("UIManager: 메인캔버스를 찾지 못함");
            }

            if (popupCanvas != null)
            {
                _popupCanvas = popupCanvas.transform;
            }
            else
            {
                _popupCanvas = new GameObject("PopupCanvas").transform;
            }

            // 게임 상태와 Address 맵핑
            /*Dictionary<CurrentState, string> uiAddressMap = new Dictionary<CurrentState, string>();
            uiAddressMap.Add(CurrentState.Lobby, LOBBY_UI_ADDRESS);
            uiAddressMap.Add(CurrentState.Loading, LOADING_UI_ADDRESS);
            uiAddressMap.Add(CurrentState.InGame, INGAME_UI_ADDRESS);
            uiAddressMap.Add(CurrentState.Setting, SETTING_UI_ADDRESS);
            uiAddressMap.Add(CurrentState.Inventory, INVENTORY_UI_ADDRESS);
            uiAddressMap.Add(CurrentState.Hacking, HACKING_UI_ADDRESS);
            uiAddressMap.Add(CurrentState.WeaponTuning, WEAPONTUNING_UI_ADDRESS);
            uiAddressMap.Add(CurrentState.Menu, MENU_UI_ADDRESS);*/
            
            ChangeState(CurrentState.Lobby);
        }

        public async Task<T> ShowPopup<T>(string address) where T : UIPopup
        {
            if (popupCache.TryGetValue(address, out UIPopup cachedPopup))
            {
                popupStack.Push(cachedPopup);
                cachedPopup.gameObject.SetActive(true);
                cachedPopup.transform.SetAsLastSibling();
                return cachedPopup as T;
            }

            var handle = Addressables.LoadAssetAsync<GameObject>(address);
            GameObject popupPanel = await handle.Task;

            if (popupPanel != null)
            {
                GameObject popupInstance = UnityEngine.Object.Instantiate(popupPanel, _popupCanvas);
                T popupComponent = popupInstance.GetComponent<T>();

                if (popupComponent != null)
                {
                    popupComponent.Init(this);
                    popupCache[address] = popupComponent;
                    popupStack.Push(popupComponent);
                    popupComponent.transform.SetAsLastSibling();
                    return popupComponent;
                }
                else
                {
                    UnityEngine.Object.Destroy(popupInstance);
                    return null;
                }
            }
            return null;
        }

        public void ClosePopup(UIPopup popup)
        {
            if (popupStack.Count > 0 && popupStack.Peek() == popup)
            {
                popupStack.Pop();
                popup.gameObject.SetActive(false);
            }
        }

        public void CloseTopPopup()
        {
            if (popupStack.Count > 0)
            {
                UIPopup topPopup = popupStack.Pop();
                topPopup.gameObject.SetActive(false);
            }
        }

        public async void ChangeState(CurrentState newState)
        {
            // 화면에 띄울 UI를 게임 상태가 변경됨에 따라 교체해준다
            if (currentState == newState) return;

            if (currentState != CurrentState.None)
            {
                if (loadedUI.TryGetValue(currentState, out var previousPanel))
                {
                    previousPanel.SetActive(false);
                }
            }
            
            previousState = currentState;
            currentState = newState;

            if (!loadedUI.ContainsKey(newState) || loadedUI[newState] == null)
            {
                await LoadUI(newState);
            }
            else
            {
                loadedUI[newState].SetActive(true);
            }
        }

        public CurrentState GetCurrentState()
        {
            return currentState;
        }

        public CurrentState GetPreviousState()
        {
            return previousState;
        }

        private async Task LoadUI(CurrentState state)
        {
            string address = GetUIAddress(state);
            if (string.IsNullOrEmpty(address)) return;

            if (loadingOperation.ContainsKey(state) && loadingOperation[state].IsValid())
            {
                await loadingOperation[state].Task;
                if (loadedUI.TryGetValue(state, out var loadedPanel) && loadedPanel != null)
                {
                    loadedPanel.SetActive(true);
                }
                return;
            }
            
            var handle = Addressables.LoadAssetAsync<GameObject>(address);
            loadingOperation[state] = handle;
            
            GameObject panel = await handle.Task;

            if (panel != null)
            {
                GameObject uiInstance = UnityEngine.Object.Instantiate(panel, _mainCanvas);
                UIBase uiBase = uiInstance.GetComponent<UIBase>();
                if (uiBase != null)
                {
                    uiBase.Init(this);
                    loadedUI[state] = uiBase;
                    uiBase.SetActive(true);
                }
                else
                {
                    UnityEngine.Object.Destroy(uiInstance);
                }
            }
            else
            {
                Service.Log($"UIManager: {state}에 맞는 UI프리팹 로드 실패");
            }

            loadingOperation.Remove(state);
        }

        private string GetUIAddress(CurrentState state)
        {
            switch (state)
            {
                case CurrentState.Lobby: return LOBBY_UI_ADDRESS;
                case CurrentState.Loading: return LOADING_UI_ADDRESS;
                case CurrentState.InGame: return INGAME_UI_ADDRESS;
                case CurrentState.Setting: return SETTING_UI_ADDRESS;
                case CurrentState.Inventory: return INVENTORY_UI_ADDRESS;
                case CurrentState.Hacking: return HACKING_UI_ADDRESS;
                case CurrentState.WeaponTuning: return WEAPONTUNING_UI_ADDRESS;
                case CurrentState.Menu: return MENU_UI_ADDRESS;
                default: return null;
            }
        }
    }
}
