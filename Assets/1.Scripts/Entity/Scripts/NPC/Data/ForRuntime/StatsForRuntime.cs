using _1.Scripts.Entity.Scripts.Common;
using _1.Scripts.Entity.Scripts.NPC.Data;
using _1.Scripts.Interfaces;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.Data.ForRuntime
{
    /// <summary>
    /// SO를 조작하면 전체가 바뀌기에, 복사본을 생성하는 클래스들 모음
    /// </summary>
    public class RuntimeEntityStatData
    {
        public string entityName;
        public bool isPlayer;
        public bool isAlly;

        public int maxHealth;
        public int baseDamage;
        public float baseAttackRate;

        public float moveSpeed;
        public float runMultiplier;
        public float walkMultiplier;

        public AudioClip[] footStepSounds;
        public AudioClip[] hitSounds;
        public AudioClip[] deathSounds;
        
        protected RuntimeEntityStatData(EntityStatData so)
        {
            entityName = so.entityName;
            isPlayer = so.isPlayer;
            isAlly = so.isAlly;

            maxHealth = so.maxHealth;
            baseDamage = so.baseDamage;
            baseAttackRate = so.baseAttackRate;

            moveSpeed = so.moveSpeed;
            runMultiplier = so.runMultiplier;
            walkMultiplier = so.walkMultiplier;

            footStepSounds = so.footStepSounds;
            hitSounds = so.hitSounds;
            deathSounds = so.deathSounds;
        }
    }
    
    public class RuntimeReconDroneStatData : RuntimeEntityStatData, IDetectable, IAttackable, IAlertable
    {
        public float DetectRange { get; set; }
        public float AttackRange { get; set; }
        public float AlertDuration { get; set; }
        public float AlertRadius { get; set; }

        // 생성자: ReconDroneData(SO)에서 값 복사 + 베이스 생성자 호출
        public RuntimeReconDroneStatData(ReconDroneStatData so) : base(so)
        {
            DetectRange = so.DetectRange;
            AttackRange = so.AttackRange;
            AlertDuration = so.AlertDuration;
            AlertRadius = so.AlertRadius;
        }
    }
}