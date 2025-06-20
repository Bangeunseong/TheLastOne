using System;
using _1.Scripts.Manager.Core;
using UnityEngine;

namespace _1.Scripts.Manager.Subs
{
    [Serializable] public class SceneLoadManager
    {
        [Header("Core")]
        [SerializeField] private Managers coreManager;
        
        // Constructor
        public SceneLoadManager(Managers core){ coreManager = core; }
    }
}
