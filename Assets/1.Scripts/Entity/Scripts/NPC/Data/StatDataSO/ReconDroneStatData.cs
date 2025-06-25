using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Common;
using _1.Scripts.Interfaces;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.Data
{
    [CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/Entity/Enemy/ReconDroneData")]
    public class ReconDroneStatData : EntityStatData, IDetectable, IAttackable, IAlertable
    {
        [Header("Range")] 
        [SerializeField] private float detectRange;
        [SerializeField] private float attackRange;
        
        [Header("Alert")]
        [SerializeField] private float alertDuration;
        [SerializeField] private float alertRadius;
        
        public float DetectRange => detectRange;
        public float AttackRange => attackRange;
        public float AlertDuration => alertDuration;
        public float AlertRadius => alertRadius;
    }
}



