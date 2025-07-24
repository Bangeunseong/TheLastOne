using System;
using System.Collections.Generic;
using _1.Scripts.Manager.Core;
using _1.Scripts.UI;
using _1.Scripts.UI.Common;
using _1.Scripts.UI.InGame;
using _1.Scripts.UI.InGame.Dialogue;
using _1.Scripts.UI.InGame.Mission;
using _1.Scripts.UI.InGame.Quest;
using _1.Scripts.UI.Inventory;
using _1.Scripts.UI.Loading;
using _1.Scripts.UI.Lobby;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace _1.Scripts.Manager.Subs
{
    public enum UIType
    {
        Persistent,
        InGame,
        InGame_HUD,
    }
    [Serializable] public class UIManager
    {
        [field: Header("UI Mapping")]
        [field: SerializeField] public Canvas RootCanvas { get; private set; }
        [field: SerializeField] public Transform UiRoot { get; private set; }
        
        private readonly Dictionary<Type, UIBase> uiMap = new();
        private readonly Dictionary<UIType, List<Type>> uiGroupMap = new()
        {
            {
                UIType.Persistent, new List<Type> { typeof(LoadingUI), typeof(FadeUI), typeof(LobbyUI) }
            },
            { 
                UIType.InGame, new List<Type> { typeof(InGameUI), typeof(DistanceUI), typeof(WeaponUI),
                typeof(QuickSlotUI), typeof(QuestUI), typeof(MinigameUI), typeof(InventoryUI), 
                typeof(PauseMenuUI), typeof(DialogueUI), typeof(GameOverUI) } 
            },
            {
                UIType.InGame_HUD, new List<Type>{typeof(InGameUI), typeof(DistanceUI), typeof(QuestUI), typeof(WeaponUI),}
            }
        };
        private CoreManager coreManager;
        
        public bool IsCutscene { get; private set; }
        
        public void Start()
        {
            coreManager = CoreManager.Instance;
            var canvas = GameObject.FindGameObjectWithTag("MainCanvas");
            if (canvas)
            {
                RootCanvas = canvas.GetComponent<Canvas>();
                UiRoot = canvas.transform;
            }
            
            RegisterStaticUI<LoadingUI>(); RegisterStaticUI<LobbyUI>(); RegisterStaticUI<FadeUI>();
            ShowUI<LobbyUI>(); GetUI<FadeUI>().FadeIn();
        }

        private bool RegisterStaticUI<T>() where T : UIBase
        {
            var ui = Object.FindObjectOfType<T>(true);
            if (!ui || uiMap.TryGetValue(typeof(T), out var val)) return false;
            
            ui.Initialize(this);
            uiMap.Add(typeof(T), ui);
            return true;
        }
        
        public bool RegisterDynamicUI<T>() where T : UIBase
        {
            if (uiMap.ContainsKey(typeof(T))) return false;
            
            var uiResource = coreManager.resourceManager.GetAsset<GameObject>(typeof(T).Name);
            if (!uiResource) return false;
            if (!uiResource.TryGetComponent(out T dynamicUI)) return false;
            if (!uiMap.TryAdd(typeof(T), dynamicUI))
            {
                if (!uiMap.TryGetValue(typeof(T), out var ui)) return false;
                ui.Initialize(this, coreManager.gameManager.Player.PlayerCondition);
                return true;
            }

            Object.Instantiate(dynamicUI, UiRoot);
            dynamicUI.Initialize(this, coreManager.gameManager.Player.PlayerCondition);
            return true;
        }

        public bool RegisterDynamicUIByGroup(UIType groupType)
        {
            if (!uiGroupMap.TryGetValue(groupType, out var value)) return false;
            foreach (var type in value)
            {
                var method = typeof(UIManager).GetMethod(nameof(RegisterDynamicUI))?.MakeGenericMethod(type);
                method?.Invoke(this, null);
            }
            return true;
        }

        public bool UnregisterDynamicUI<T>() where T : UIBase
        {
            if (!uiMap.TryGetValue(typeof(T), out var dynamicUI)) return false;
            
            uiMap.Remove(typeof(T));
            Object.Destroy(dynamicUI.gameObject);
            return true;
        }

        public bool UnregisterDynamicUIByGroup(UIType groupType)
        {
            if (!uiGroupMap.TryGetValue(groupType, out var value)) return false;
            foreach (var type in value)
            {
                var method = typeof(UIManager).GetMethod(nameof(UnregisterDynamicUI))?.MakeGenericMethod(type);
                method?.Invoke(this, null);
            }
            return true;
        }
        
        public T GetUI<T>() where T : UIBase
        {
            return uiMap.TryGetValue(typeof(T), out var ui) ? ui as T : null;
        }

        public bool ShowUI<T>() where T : UIBase
        {
            if (!uiMap.TryGetValue(typeof(T), out var ui)) return false;
            ui.Show();
            return true;
        }

        public bool ShowHUD()
        {
            if (!uiGroupMap.TryGetValue(UIType.InGame_HUD, out var value)) return false;
            foreach (var type in value)
            {
                var method = typeof(UIManager).GetMethod(nameof(ShowUI))?.MakeGenericMethod(type);
                method?.Invoke(this, null);
            }
            return true;
        }

        public bool ShowUIByGroup(UIType groupType)
        {
            if (!uiGroupMap.TryGetValue(groupType, out var value)) return false;
            foreach (var type in value)
            {
                var method = typeof(UIManager).GetMethod(nameof(ShowUI))?.MakeGenericMethod(type);
                method?.Invoke(this, null);
            }
            return true;
        }

        public bool ShowPauseMenu()
        {
            if (!HideUIByGroup(UIType.InGame)) return false;
            ShowUI<PauseMenuUI>();
            return true;
        }

        public bool HideUI<T>() where T : UIBase
        {
            if (!uiMap.TryGetValue(typeof(T), out var ui)) return false;
            ui.Hide();
            return true;
        }

        public bool HideHUD()
        {
            if (!uiGroupMap.TryGetValue(UIType.InGame_HUD, out var value)) return false;
            foreach (var type in value)
            {
                var method = typeof(UIManager).GetMethod(nameof(HideUI))?.MakeGenericMethod(type);
                method?.Invoke(this, null);
            }
            return true;
        }

        public bool HideUIByGroup(UIType groupType)
        {
            if (!uiGroupMap.TryGetValue(groupType, out var value)) return false;
            foreach (var type in value)
            {
                var method = typeof(UIManager).GetMethod(nameof(HideUI))?.MakeGenericMethod(type);
                method?.Invoke(this, null);
            }
            return true;
        }
        
        public bool HidePauseMenu()
        {
            return HideUIByGroup(UIType.InGame) && ShowHUD();
        }

        public void ResetUI<T>() where T : UIBase
        {
            if (!uiMap.TryGetValue(typeof(T), out var ui)) return;
            ui.ResetUI();
        }
        
        public bool ResetHUD()
        {
            if (!uiGroupMap.TryGetValue(UIType.InGame_HUD, out var value)) return false;
            foreach (var type in value)
            {
                var method = typeof(UIManager).GetMethod(nameof(ResetUI))?.MakeGenericMethod(type);
                method?.Invoke(this, null);
            }
            return true;
        }

        public bool ResetUIByGroup(UIType groupType)
        {
            if (!uiGroupMap.TryGetValue(groupType, out var value)) return false;
            foreach (var type in value)
            {
                var method = typeof(UIManager).GetMethod(nameof(ResetUI))?.MakeGenericMethod(type);
                method?.Invoke(this, null);
            }
            return true;
        }
        
        public void OnCutsceneStarted(PlayableDirector _)
        {
            IsCutscene = true;
            HideUIByGroup(UIType.InGame);
        }
        
        public void OnCutsceneStopped(PlayableDirector director)
        {
            director.played -= OnCutsceneStarted;
            director.stopped -= OnCutsceneStopped;
            IsCutscene = false;
            if (!ShowHUD()) throw new MissingReferenceException();
        }

        private void InjectHandler(UIBase ui)
        {
            var menuHandler = GameObject.FindObjectOfType<MenuHandler>();

            if (ui is InventoryUI inventoryUI)
            {
                var inventoryHandler = inventoryUI.GetComponent<InventoryHandler>();
                if (menuHandler && inventoryHandler)
                    menuHandler.SetInventoryHandler(inventoryHandler);

                var pauseHandler = CoreManager.Instance.uiManager.GetUI<PauseMenuUI>()?.GetComponent<PauseHandler>();
                if (pauseHandler)
                {
                    pauseHandler.SetInventoryHandler(inventoryHandler);
                    inventoryHandler.SetPauseHandler(pauseHandler);
                }
            }

            if (ui is PauseMenuUI pauseMenuUI)
            {
                var pauseHandler = pauseMenuUI.GetComponent<PauseHandler>();
                if (menuHandler && pauseHandler)
                    menuHandler.SetPauseHandler(pauseHandler);
                pauseHandler.SetPauseMenuUI(pauseMenuUI);

                var inventoryHandler = CoreManager.Instance.uiManager.GetUI<InventoryUI>()?.GetComponent<InventoryHandler>();
                if (inventoryHandler)
                {        
                    pauseHandler.SetInventoryHandler(inventoryHandler);
                    inventoryHandler.SetPauseHandler(pauseHandler);
                }
            }
        }
    }
}