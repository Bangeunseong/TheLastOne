using System;
using _1.Scripts.Manager.Core;
using UnityEngine;

namespace _1.Scripts.Manager.Subs
{
    [Serializable] public class GameManager
    {
        // Fields
        [Header("Core")]
        [SerializeField] private Managers coreManager;

        // Constructor
        public GameManager(Managers core){ coreManager = core; }
        
        // Methods
        
    }
}
