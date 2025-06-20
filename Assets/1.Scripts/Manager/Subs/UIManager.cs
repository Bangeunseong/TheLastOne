using System;
using _1.Scripts.Manager.Core;
using UnityEngine;

namespace _1.Scripts.Manager.Subs
{
    [Serializable] public class UIManager
    {
        [Header("Core")]
        [SerializeField] private CoreManager coreCoreManager;
        
        // Constructor
        public UIManager(CoreManager coreCore){ coreCoreManager = coreCore; }
    }
}
