using System;
using System.Collections.Generic;
using _1.Scripts.Dialogue;
using _1.Scripts.Manager.Core;
using _1.Scripts.UI.InGame.Dialogue;
using UnityEngine;

namespace _1.Scripts.Manager.Subs
{
    [Serializable] public class DialogueManager
    {
        [SerializeField] private List<DialogueDataSO> dialogueDataList = new();
        private Dictionary<string, DialogueDataSO> dialogueDataDict = new();
        private CoreManager coreManager;

        public void Start()
        {
            coreManager = CoreManager.Instance;

        }

        public void CacheDialogueData()
        {
            dialogueDataDict = new Dictionary<string, DialogueDataSO>();
            dialogueDataList.Clear();
            
            var loadedSO = coreManager.resourceManager.GetAllAssetsOfType<DialogueDataSO>();
            if (loadedSO == null || loadedSO.Count == 0) return;
            
            foreach (var so in loadedSO)
            {
                if (so && !dialogueDataDict.ContainsKey(so.dialogueKey))
                {
                    dialogueDataList.Add(so);
                    dialogueDataDict.Add(so.dialogueKey, so);
                }
            }
        }

        public void TriggerDialogue(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            
            if (dialogueDataDict.TryGetValue(key, out var data))
            {
                var dialogueUI = coreManager.uiManager.GetUI<DialogueUI>();
                if (dialogueUI)
                {
                    dialogueUI.ShowSequence(data.sequence);
                }
                else
                {
                    Debug.LogWarning("DialogueUI is not available.");
                }
            }
            else
            {
                Debug.LogWarning($"Dialogue key not found: {key}");
            }
        }
    }
}