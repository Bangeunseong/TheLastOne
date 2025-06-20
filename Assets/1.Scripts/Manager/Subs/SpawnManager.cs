using System;
using _1.Scripts.Manager.Core;
using UnityEngine;

namespace _1.Scripts.Manager.Subs
{
    [Serializable] public class SpawnManager
    {
        [Header("Core")]
        [SerializeField] private CoreManager coreCoreManager;
        
        // Constructor
        public SpawnManager(CoreManager coreCore){ coreCoreManager = coreCore; }
    }
}
