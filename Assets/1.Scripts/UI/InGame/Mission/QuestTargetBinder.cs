using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using UnityEngine;
using UnityEngine.Analytics;


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
        [Header("Quest Target Bindings")]
        [SerializeField] private List<QuestTargetBinding> bindings;
        
        private QuestManager questManager;
        
        // Singleton
        public static QuestTargetBinder Instance { get; private set; }

        private void Awake()
        {
            if (!Instance) Instance = this;
            else { if(Instance != this) Destroy(gameObject); }
        }
        
        public void SetCurrentTarget(int targetId)
        {
            var binding = bindings.Find(x => x.questID == targetId);
            if (binding == null) 
                return;
            
            CoreManager.Instance.uiManager.InGameUI.DistanceUI.SetTarget(binding.target);
        }
    }
}