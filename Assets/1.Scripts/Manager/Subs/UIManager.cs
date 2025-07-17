using System;
using System.Collections.Generic;
using _1.Scripts.Manager.Core;
using _1.Scripts.UI;
using _1.Scripts.UI.InGame;
using _1.Scripts.UI.InGame.Mission;
using _1.Scripts.UI.Inventory;
using _1.Scripts.UI.Loading;
using _1.Scripts.UI.Lobby;
using _1.Scripts.UI.Setting;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace _1.Scripts.Manager.Subs
{
    [Serializable] public class UIManager
    {
        private Dictionary<Type, UIBase> uiMap = new();
        private Transform uiRoot;
        private CoreManager coreManager;
        private Dictionary<UIBase, bool> UIStateCache = new();
        public void Start()
        {
            coreManager = CoreManager.Instance;
            var canvas = GameObject.FindGameObjectWithTag("MainCanvas");
            if (canvas) uiRoot = canvas.transform;
            
            RegisterStaticUI<LoadingUI>();
            RegisterStaticUI<LobbyUI>();
        }
        
        public T GetUI<T>() where T : UIBase
        {
            return uiMap.TryGetValue(typeof(T), out var ui) ? ui as T : null;
        }

        public T ShowUI<T>() where T : UIBase
        {
            var ui = GetUI<T>() ?? LoadUI<T>();
            ui?.Show();
            return ui;
        }
        
        public void HideUI<T>() where T : UIBase
        {
            var ui = GetUI<T>();
            ui?.Hide();
        }

        private T LoadUI<T>() where T : UIBase
        {
            string address = typeof(T).Name;
            var prefab = coreManager.resourceManager.GetAsset<GameObject>(address);
            if (!prefab) return null;
            var instance = Object.Instantiate(prefab, uiRoot, false);
            if (!instance.TryGetComponent(out T component)) return null;
            component.Init(this);
            uiMap[typeof(T)] = component;
            return component;
        }

        private void RegisterStaticUI<T>() where T : UIBase
        {
            var ui = GameObject.FindObjectOfType<T>(true);
            if (ui != null && !uiMap.ContainsKey(typeof(T)))
            {
                ui.Init(this);
                uiMap.Add(typeof(T), ui);
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
            if (!uiMap.TryGetValue(typeof(T), out var ui)) return;
            Object.Destroy(ui.gameObject);
            uiMap.Remove(typeof(T));
        }
        
        public void HideAndSaveAllUI()
        {
            UIStateCache.Clear();
            foreach (var ui in uiMap.Values)
            {
                UIStateCache[ui] = ui.gameObject.activeSelf;
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
        
        public void OnCutsceneStarted(UnityEngine.Playables.PlayableDirector _)
        {
            HideAndSaveAllUI();
        }
        public void OnCutsceneStopped(UnityEngine.Playables.PlayableDirector _)
        {
            RestoreAllUI();
        }
    }
}