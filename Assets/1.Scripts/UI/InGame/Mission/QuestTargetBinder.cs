using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using UnityEngine;


namespace _1.Scripts.UI.InGame.Mission
{
    [Serializable]
    public class QuestTargetBinding
    {
        public int questID;
        public Transform target;
    }
    public class QuestTargetBinder : MonoBehaviour
    {
        private QuestManager questManager;
        [SerializeField] private List<QuestTargetBinding> bindings;


        private int lastTargetIndex = -1;


        private void Start()
        {
            if (questManager == null) questManager = CoreManager.Instance.questManager;
            if (bindings == null || bindings.Count == 0) return;

            lastTargetIndex = 0;
            SetCurrentTarget(bindings[lastTargetIndex].target);
        }

        private void Update()
        {
            if (bindings == null || lastTargetIndex < 0 || lastTargetIndex >= bindings.Count) return;

            var binding = bindings[lastTargetIndex];

            if (questManager.activeQuests.TryGetValue(binding.questID, out var quest) && quest.isCompleted)
            {
                lastTargetIndex++;

                if (lastTargetIndex >= bindings.Count)
                {
                    SetCurrentTarget(bindings[lastTargetIndex].target);
                }
                else
                {
                    CoreManager.Instance.uiManager.SetDistanceTarget(null);
                }
            }
        }
        
        private void SetCurrentTarget(Transform target)
        {
            CoreManager.Instance.uiManager.SetDistanceTarget(target);
        }
    }
}