using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Common;
using _1.Scripts.Interfaces;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.Data
{
    [CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/Entity/Enemy/StandardDroneStat")]
    public class StandardDroneStat : EntityStatData, IDetectable, IAttackable
    {
        [Header("Range")] 
        public float detectRange;
        public float attackRange;
        
        public float DetectRange => detectRange;
        public float AttackRange => attackRange;
    }
}
