using System;
using System.Collections.Generic;
using _1.Scripts.Manager.Core;
using _1.Scripts.UI;
using _1.Scripts.UI.Common;
using _1.Scripts.UI.Inventory;
using _1.Scripts.UI.Loading;
using _1.Scripts.UI.Lobby;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace _1.Scripts.Manager.Subs
{
    [Serializable]
    public class UIManager
    {
        [field: Header("UI Mapping")]
        [field: SerializeField] public Canvas RootCanvas { get; private set; }
        [SerializeField] private Transform uiRoot;
        
        public bool IsCutscene { get; private set; }
        
        private Dictionary<Type, UIBase> uiMap = new();
        private Dictionary<UIBase, bool> UIStateCache = new();
        private CoreManager coreManager;
        
        public void Start()
        {
            coreManager = CoreManager.Instance;
            var canvas = GameObject.FindGameObjectWithTag("MainCanvas");
            if (canvas)
            {
                RootCanvas = canvas.GetComponent<Canvas>();
                uiRoot = canvas.transform;
            }
            
            RegisterStaticUI<LoadingUI>();
            RegisterStaticUI<LobbyUI>();
            RegisterStaticUI<FadeUI>();
            ShowUI<LobbyUI>();
            GetUI<FadeUI>().FadeIn();
        }
        
        public T GetUI<T>() where T : UIBase
        {
            return uiMap.TryGetValue(typeof(T), out var ui) ? ui as T : null;
        }

        public T ShowUI<T>() where T : UIBase
        {
            var ui = GetUI<T>() ?? LoadUI<T>();
            ui.Show();
            InjectHandler(ui);
            return ui;
        }
        
        public void HideUI<T>() where T : UIBase
        {
            var ui = GetUI<T>();
            if (ui) ui.Hide();
            else Debug.Log($"{typeof(T).Name}이 uiMap에 없음");
        }

        public T LoadUI<T>() where T : UIBase
        {
            string address = typeof(T).Name;
            var prefab = coreManager.resourceManager.GetAsset<GameObject>(address);
            if (!prefab) return null;
            var instance = Object.Instantiate(prefab, uiRoot, false);
            if (!instance.TryGetComponent(out T component)) return null;
            component.Init(this);
            uiMap[typeof(T)] = component;
            Service.Log($"UI {typeof(T).Name} Registered");
            return component;
        }
        
        private void RegisterStaticUI<T>() where T : UIBase
        {
            var ui = GameObject.FindObjectOfType<T>(true);
            if (ui != null && !uiMap.ContainsKey(typeof(T)))
            {
                ui.Init(this);
                uiMap.Add(typeof(T), ui);
                Service.Log($"UI {typeof(T).Name} Registered");
            }
        }

        public void ResetUI()
        {
            foreach (var ui in uiMap.Values)
            {
                ui.Hide();
                ui.ResetUI();
            }
        }

        public void InitializeUI<T>(object param) where T : UIBase
        {
            var ui = GetUI<T>();
            ui?.Initialize(param);
        }
        
        public void InitializeAllUI(object param)
        {
            foreach (var ui in uiMap.Values)
            {
                ui.Initialize(param);
            }
        }
        
        public void UnloadUI<T>() where T : UIBase
        {
            Debug.Log($"UnloadUI {typeof(T).Name}");
            if (!uiMap.TryGetValue(typeof(T), out var ui)) return;
            Object.Destroy(ui.gameObject);
            uiMap.Remove(typeof(T));
        }
        
        public void HideAndSaveAllUI()
        {
            UIStateCache.Clear();
            foreach (var ui in uiMap.Values)
            {
                UIStateCache[ui] = ui.gameObject.activeInHierarchy;
                ui.Hide();
            }
        }
        
        public void RestoreAllUI()
        {
            foreach (var kvp in UIStateCache)
            {
                if (kvp.Value) kvp.Key.Show();
            }
            UIStateCache.Clear();
        }
        
        public void OnCutsceneStarted(PlayableDirector _)
        {
            IsCutscene = true;
            HideAndSaveAllUI();
        }
        public void OnCutsceneStopped(PlayableDirector director)
        {
            director.played -= OnCutsceneStarted;
            director.stopped -= OnCutsceneStopped;
            IsCutscene = false;
            RestoreAllUI();
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